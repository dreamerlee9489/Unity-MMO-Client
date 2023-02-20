using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Manage
{
    public enum AppType
    {
        Client,
        Login,
        Game
    }

    public enum NetState
    {
        NoConnect,
        Connecting,
        Connected,
        Disconnected,
    }

    public struct HttpJson
    {
        public string ip;
        public int port;
        public int returncode;
    }

    public class PacketHead
    {
        public const ushort SIZE = 6;
        public ushort msgId = 0;
    }

    public class NetManager : MonoSingleton<NetManager>
    {
        public delegate Google.Protobuf.IMessage ParseFunc(byte[] bytes, int offset, int length);

        private Socket _sock = null;
        private NetState _state = NetState.NoConnect;
        private int _recvIdx = 0;
        private readonly byte[] _recvBuf = new byte[512 * 1024];
        private readonly Dictionary<Proto.MsgId, ParseFunc> _funcDict = new();

        public string remoteIp;
        public int remotePort;

        public AppType CurApp { get; private set; } = AppType.Client;

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
            RegistParseFunc(Proto.MsgId.S2CAllRoleAppear, ParsePacket<Proto.AllRoleAppear>);
            RegistParseFunc(Proto.MsgId.S2CReqNpcInfo, ParsePacket<Proto.ReqNpcInfo>);
            RegistParseFunc(Proto.MsgId.S2CSyncPlayerProps, ParsePacket<Proto.SyncPlayerProps>);
            RegistParseFunc(Proto.MsgId.S2CSyncNpcProps, ParsePacket<Proto.SyncNpcProps>);
            RegistParseFunc(Proto.MsgId.S2CSyncFsmState, ParsePacket<Proto.SyncFsmState>);
            RegistParseFunc(Proto.MsgId.S2CSyncPlayerPos, ParsePacket<Proto.SyncPlayerPos>);
            RegistParseFunc(Proto.MsgId.S2CSyncPlayerCmd, ParsePacket<Proto.SyncPlayerCmd>);
            RegistParseFunc(Proto.MsgId.S2CRoleDisappear, ParsePacket<Proto.RoleDisappear>);
            RegistParseFunc(Proto.MsgId.S2CReqLinkPlayer, ParsePacket<Proto.ReqLinkPlayer>);
            RegistParseFunc(Proto.MsgId.S2CPlayerAtkEvent, ParsePacket<Proto.PlayerAtkEvent>);
            RegistParseFunc(Proto.MsgId.S2CNpcAtkEvent, ParsePacket<Proto.NpcAtkEvent>);
            RegistParseFunc(Proto.MsgId.S2CDropItemList, ParsePacket<Proto.DropItemList>);
            RegistParseFunc(Proto.MsgId.S2CGetPlayerKnap, ParsePacket<Proto.PlayerKnap>);
            RegistParseFunc(Proto.MsgId.MiGlobalChat, ParsePacket<Proto.ChatMsg>);
            RegistParseFunc(Proto.MsgId.MiWorldChat, ParsePacket<Proto.ChatMsg>);
            RegistParseFunc(Proto.MsgId.MiTeamChat, ParsePacket<Proto.ChatMsg>);
            RegistParseFunc(Proto.MsgId.MiPrivateChat, ParsePacket<Proto.ChatMsg>);
            RegistParseFunc(Proto.MsgId.MiCreateTeam, ParsePacket<Proto.CreateTeam>);
            RegistParseFunc(Proto.MsgId.C2CReqEnterDungeon, ParsePacket<Proto.EnterDungeon>);
            RegistParseFunc(Proto.MsgId.C2CReqJoinTeam, ParsePacket<Proto.PlayerReq>);
            RegistParseFunc(Proto.MsgId.C2CJoinTeamRes, ParsePacket<Proto.PlayerReq>);
            RegistParseFunc(Proto.MsgId.C2CReqPvp, ParsePacket<Proto.PlayerReq>);
            RegistParseFunc(Proto.MsgId.C2CPvpRes, ParsePacket<Proto.PlayerReq>);            
            RegistParseFunc(Proto.MsgId.C2CReqTrade, ParsePacket<Proto.PlayerReq>);
            RegistParseFunc(Proto.MsgId.C2CUpdateTradeItem, ParsePacket<Proto.UpdateTradeItem>);
            RegistParseFunc(Proto.MsgId.S2CTradeOpen, ParsePacket<Proto.TradeOpen>);
            RegistParseFunc(Proto.MsgId.S2CTradeClose, ParsePacket<Proto.TradeClose>);
            RegistParseFunc(Proto.MsgId.S2CSyncBtAction, ParsePacket<Proto.SyncBtAction>);
            RegistParseFunc(Proto.MsgId.S2CPlayerMove, ParsePacket<Proto.EntityMove>);
            RegistParseFunc(Proto.MsgId.S2CNpcMove, ParsePacket<Proto.EntityMove>);
            InvokeRepeating(nameof(SendPingMsg), 10, 10);
            EventManager.Instance.AddListener<bool>(EventId.HotUpdated, HotUpdatedCallback);
        }

        private void Update()
        {
            if (_sock == null || _state != NetState.Connected)
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
                    PacketHead head = new();
                    MemoryStream ms = new(_recvBuf);
                    BinaryReader reader = new(ms);
                    int totalLen = reader.ReadUInt16();
                    int headLen = reader.ReadUInt16();
                    head.msgId = reader.ReadUInt16();
                    if (totalLen > _recvIdx)
                        break;
                    UnPacket(head, _recvBuf, (int)ms.Position, totalLen - PacketHead.SIZE);
                    if (_state != NetState.Connected)
                        return;
                    _recvIdx -= totalLen;
                    if (_recvIdx > 0)
                        Array.Copy(_recvBuf, totalLen, _recvBuf, 0, _recvIdx);
                }
            }
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener<bool>(EventId.HotUpdated, HotUpdatedCallback);
            Disconnect();
        }

        public void Connect(string ip, int port, AppType appType)
        {
            _state = NetState.Connecting;
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _sock.Blocking = true;
            _sock.SendBufferSize = _recvBuf.Length;
            EventManager.Instance.Invoke(EventId.Connecting);
            _sock.BeginConnect(ip, port, (result) =>
            {
                try
                {
                    if (result.IsCompleted)
                    {
                        _sock = result.AsyncState as Socket;
                        if (_sock != null && _sock.Connected)
                        {
                            _sock.EndConnect(result);
                            _sock.Blocking = false;
                            _state = NetState.Connected;
                            CurApp = appType;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Connect failure: " + ex.Message);
                    Disconnect();
                }
            }, _sock);
            EventManager.Instance.InvokeDelay(EventId.Connected);
        }

        public void Disconnect()
        {
            if (_sock != null)
            {
                _sock.Close();
                _sock = null;
                _state = NetState.Disconnected;
                EventManager.Instance.Invoke(EventId.Disconnect);
            }
        }

        public bool SendPacket(Proto.MsgId msgId, Google.Protobuf.IMessage msg)
        {
            int size = PacketHead.SIZE;
            size += msg != null ? msg.CalculateSize() : 0;
            MemoryStream ms = new();
            BinaryWriter writer = new(ms);
            writer.Write((ushort)size);
            writer.Write((ushort)2);
            writer.Write((ushort)msgId);

            if (msg != null)
            {
                Google.Protobuf.CodedOutputStream os = new(ms);
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
            //Debug.Log("Send msgid: " + msgId);
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
            //Debug.Log("Recv msgid: " + msgId);
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
            T msg = new();
            Google.Protobuf.CodedInputStream stream = new(bytes, offset, length);
            msg.MergeFrom(stream);
            return msg;
        }

        private void SendPingMsg()
        {
            if (_state == NetState.Connected)
                SendPacket(Proto.MsgId.MiPing, null);
        }

        public string Md5(byte[] data)
        {
            byte[] bin;
            using (MD5CryptoServiceProvider md5Crypto = new())
                bin = md5Crypto.ComputeHash(data);
            return BitConverter.ToString(bin).Replace("-", "").ToLower();
        }

        private void HotUpdatedCallback(bool updateOver)
        {
            UIManager.Instance.GetPanel<StartPanel>().Open();
            if (updateOver)
                MonoManager.Instance.StartCoroutine(ConnectServer());
            else
                UIManager.Instance.GetPanel<ModalPanel>().Open("检查更新", "更新失败！", ModalPanelType.Hint);
        }

        private IEnumerator ConnectServer()
        {
            UnityWebRequest request = UnityWebRequest.Get($"http://{remoteIp}:{remotePort}/login");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log("ConnectServer error: " + request.error);
            else
            {
                string result = request.downloadHandler.text;
                HttpJson data = JsonMapper.ToObject<HttpJson>(result);
                if (data.returncode == 0)
                    Connect(data.ip, data.port, AppType.Login);
                else
                    Debug.Log("ConnectServer error: " + request.error);
            }
        }
    }
}
