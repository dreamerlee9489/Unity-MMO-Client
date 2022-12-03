﻿using System.IO;
using System.Net.Sockets;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using Frame;

namespace Net
{
    public class NetManager : MonoSingleton<NetManager>
    {
        Socket _sock = null;
        EAppType _appType = EAppType.Client;
        ENetState _state = ENetState.NoConnect;
        int _recvIdx = 0;
        byte[] _recvBuf = new byte[512 * 1024];

        public delegate Google.Protobuf.IMessage ParseFunc(byte[] bytes, int offset, int length);
        private readonly Dictionary<Proto.MsgId, ParseFunc> _funcDict = new();
        public EAppType AppType => _appType;

        protected override void Awake()
        {
            base.Awake();
            RegistParseFunc(Proto.MsgId.C2LAccountCheckRs, ParsePacket<Proto.AccountCheckRs>);
            RegistParseFunc(Proto.MsgId.C2LCreatePlayerRs, ParsePacket<Proto.CreatePlayerRs>);
            RegistParseFunc(Proto.MsgId.C2LSelectPlayerRs, ParsePacket<Proto.SelectPlayerRs>);
            RegistParseFunc(Proto.MsgId.S2CEnterWorld, ParsePacket<Proto.EnterWorld>);
            RegistParseFunc(Proto.MsgId.L2CGameToken, ParsePacket<Proto.GameToken>);
            RegistParseFunc(Proto.MsgId.C2GLoginByTokenRs, ParsePacket<Proto.LoginByTokenRs>);
            RegistParseFunc(Proto.MsgId.L2CPlayerList, ParsePacket<Proto.PlayerList>);
            RegistParseFunc(Proto.MsgId.G2CSyncPlayer, ParsePacket<Proto.SyncPlayer>);
            RegistParseFunc(Proto.MsgId.S2CRoleAppear, ParsePacket<Proto.RoleAppear>);
            RegistParseFunc(Proto.MsgId.S2CEnemyList, ParsePacket<Proto.EnemyList>);
            RegistParseFunc(Proto.MsgId.S2CFsmChangeState, ParsePacket<Proto.FsmChangeState>);
            RegistParseFunc(Proto.MsgId.S2CPlayerSyncState, ParsePacket<Proto.PlayerSyncState>);
            RegistParseFunc(Proto.MsgId.S2CRoleDisAppear, ParsePacket<Proto.RoleDisAppear>);
            InvokeRepeating("SendPingMsg", 10, 10);
        }

        private void Update()
        {
            if (_sock == null || _state != ENetState.Connected)
                return;

            try
            {
                while (true)
                {
                    int bufAvail = _recvBuf.Length - _recvIdx;
                    if (bufAvail <= 0 || _sock.Available <= 0)
                        break;
                    int recvSize = _sock.Receive(_recvBuf, _recvIdx, bufAvail, SocketFlags.None);
                    _recvIdx += recvSize;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Disconnect();
                return;
            }

            if (_recvIdx >= PacketHead.SIZE)
            {
                while (_recvIdx >= PacketHead.SIZE)
                {
                    PacketHead head = new PacketHead();
                    MemoryStream ms = new MemoryStream(_recvBuf);
                    BinaryReader reader = new BinaryReader(ms);
                    int totalLen = reader.ReadUInt16();
                    int headLen = reader.ReadUInt16();
                    head.msgId = reader.ReadUInt16();
                    if (totalLen > _recvIdx)
                        break;
                    UnPacket(head, _recvBuf, (int)ms.Position, totalLen - PacketHead.SIZE);
                    if (_state != ENetState.Connected)
                        return;
                    _recvIdx -= totalLen;
                    if (_recvIdx > 0)
                        Array.Copy(_recvBuf, totalLen, _recvBuf, 0, _recvIdx);
                }
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void Connect(string ip, int port, EAppType appType)
        {
            _state = ENetState.Connecting;
            EventManager.Instance.Invoke(EEventType.Connecting, appType);
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _sock.Blocking = true;
            _sock.SendBufferSize = _recvBuf.Length;
            _sock.BeginConnect(ip, port, (result) =>
            {
                try
                {
                    if (result.IsCompleted)
                    {
                        Socket sock = result.AsyncState as Socket;
                        if (sock != null && sock.Connected)
                        {
                            sock.EndConnect(result);
                            sock.Blocking = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Connect failure: " + ex.Message);
                    Disconnect();
                }
            }, _sock);
            _state = ENetState.Connected;
            _appType = appType;
            EventManager.Instance.Invoke(EEventType.Connected, _appType);
        }

        public void Disconnect()
        {
            if (_sock != null)
            {
                _sock.Close();
                _sock = null;
                _state = ENetState.Disconnected;
                EventManager.Instance.Invoke(EEventType.Disconnect, _appType);
            }
        }

        public bool SendPacket(Proto.MsgId msgId, Google.Protobuf.IMessage msg)
        {
            int size = PacketHead.SIZE;
            size += msg != null ? msg.CalculateSize() : 0;
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((ushort)size);
            writer.Write((ushort)2);
            writer.Write((ushort)msgId);

            if (msg != null)
            {
                Google.Protobuf.CodedOutputStream os = new Google.Protobuf.CodedOutputStream(ms);
                msg.WriteTo(os);
                os.Flush();
            }

            try
            {
                int pos = 0;
                byte[] buf = ms.ToArray();
                while (size > 0)
                {
                    int sent = _sock.Send(buf, pos, size, SocketFlags.None);
                    size -= sent;
                    pos += sent;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Disconnect();
                return false;
            }
            return true;
        }

        public void UnPacket(PacketHead head, byte[] bytes, int offset, int length)
        {
            Proto.MsgId msgId = (Proto.MsgId)head.msgId;
            if (!_funcDict.ContainsKey(msgId))
            {
                Debug.LogWarning("未找到解包函数: msgId = " + msgId);
                return;
            }
            Google.Protobuf.IMessage msg = _funcDict[msgId](bytes, offset, length);
            MsgManager.Instance.HandleMsg(msgId, msg);
        }

        private void RegistParseFunc(Proto.MsgId msgId, ParseFunc func)
        {
            if (!_funcDict.ContainsKey(msgId))
                _funcDict.Add(msgId, null);
            _funcDict[msgId] += func;
        }

        private Google.Protobuf.IMessage ParsePacket<T>(byte[] bytes, int offset, int length) where T : Google.Protobuf.IMessage, new()
        {
            T msg = new T();
            Google.Protobuf.CodedInputStream stream = new Google.Protobuf.CodedInputStream(bytes, offset, length);
            msg.MergeFrom(stream);
            return msg;
        }

        private void SendPingMsg()
        {
            if (_state == ENetState.Connected)
                SendPacket(Proto.MsgId.MiPing, null);
        }

        public string Md5(byte[] data)
        {
            byte[] bin;
            using (MD5CryptoServiceProvider md5Crypto = new MD5CryptoServiceProvider())
                bin = md5Crypto.ComputeHash(data);
            return BitConverter.ToString(bin).Replace("-", "").ToLower();
        }
    }
}
