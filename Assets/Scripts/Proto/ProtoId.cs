// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto_id.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Proto {

  /// <summary>Holder for reflection information generated from proto_id.proto</summary>
  public static partial class ProtoIdReflection {

    #region Descriptor
    /// <summary>File descriptor for proto_id.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ProtoIdReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5wcm90b19pZC5wcm90bxIFUHJvdG8qkRMKBU1zZ0lkEggKBE5vbmUQABIV",
            "ChFNSV9OZXR3b3JrQ29ubmVjdBABEhcKE01JX05ldHdvcmtDb25uZWN0ZWQQ",
            "AhIUChBNSV9OZXR3b3JrTGlzdGVuEAMSFwoTTUlfTmV0d29ya0xpc3Rlbktl",
            "eRAEEhgKFE1JX05ldHdvcmtEaXNjb25uZWN0EAoSGgoWTUlfTmV0d29ya0Rp",
            "c2Nvbm5lY3RFeBALEh8KG01JX05ldHdvcmtSZXF1ZXN0RGlzY29ubmVjdBAU",
            "EhYKEk1JX1JlbW92ZUNvbXBvbmVudBAyEhYKEk1JX0NyZWF0ZUNvbXBvbmVu",
            "dBAzEhMKD01JX0NyZWF0ZVN5c3RlbRA0EgsKB01JX1BpbmcQZRISCg5NSV9B",
            "cHBSZWdpc3RlchBmEhIKDk1JX0FwcEluZm9TeW5jEGcSFgoSTUlfQXBwSW5m",
            "b0xpc3RTeW5jEGgSFQoQQzJMX0FjY291bnRDaGVjaxDoBxIXChJDMkxfQWNj",
            "b3VudENoZWNrUnMQ6QcSIQocTUlfQWNjb3VudFF1ZXJ5T25saW5lVG9SZWRp",
            "cxDqBxIjCh5NSV9BY2NvdW50UXVlcnlPbmxpbmVUb1JlZGlzUnMQ6wcSGQoU",
            "TDJEQl9RdWVyeVBsYXllckxpc3QQ8gcSGwoWTDJEQl9RdWVyeVBsYXllckxp",
            "c3RScxDzBxITCg5MMkNfUGxheWVyTGlzdBD0BxIVChBDMkxfQ3JlYXRlUGxh",
            "eWVyEPYHEhcKEkMyTF9DcmVhdGVQbGF5ZXJScxD3BxIWChFMMkRCX0NyZWF0",
            "ZVBsYXllchD4BxIYChNMMkRCX0NyZWF0ZVBsYXllclJzEPkHEhUKEEMyTF9T",
            "ZWxlY3RQbGF5ZXIQ+gcSFwoSQzJMX1NlbGVjdFBsYXllclJzEPsHEhkKFE1J",
            "X0xvZ2luVG9rZW5Ub1JlZGlzEP4HEhsKFk1JX0xvZ2luVG9rZW5Ub1JlZGlz",
            "UnMQ/wcSEgoNTDJDX0dhbWVUb2tlbhCACBIVChBDMkdfTG9naW5CeVRva2Vu",
            "EMwIEhcKEkMyR19Mb2dpbkJ5VG9rZW5ScxDNCBIYChNNSV9HYW1lVG9rZW5U",
            "b1JlZGlzEM4IEhoKFU1JX0dhbWVUb2tlblRvUmVkaXNScxDPCBIVChBHMkRC",
            "X1F1ZXJ5UGxheWVyENAIEhcKEkcyREJfUXVlcnlQbGF5ZXJScxDRCBITCg5H",
            "MkNfU3luY1BsYXllchDSCBIVChBHMk1fUmVxdWVzdFdvcmxkENQIEhQKD0cy",
            "U19DcmVhdGVXb3JsZBDVCBIQCgtNSV9UZWxlcG9ydBDWCBIVChBNSV9UZWxl",
            "cG9ydEFmdGVyENcIEhMKDkcyU19TeW5jUGxheWVyENgIEhoKFUcyU19SZXF1",
            "ZXN0U3luY1BsYXllchDZCBIUCg9HMkRCX1NhdmVQbGF5ZXIQ2ggSFQoQRzJT",
            "X1JlbW92ZVBsYXllchDbCBITCg5DMkdfRW50ZXJXb3JsZBDcCBITCg5HMk1f",
            "UXVlcnlXb3JsZBDdCBIVChBHMk1fUXVlcnlXb3JsZFJzEN4IEhMKDlMyQ19F",
            "bnRlcldvcmxkEN0LEhMKDlMyR19TeW5jUGxheWVyEN4LEhYKEVMyQ19BbGxS",
            "b2xlQXBwZWFyEN8LEhYKEVMyQ19Sb2xlRGlzYXBwZWFyEOALEg0KCEMyU19N",
            "b3ZlEOELEg0KCFMyQ19Nb3ZlEOILEhQKD1MyQ19QbGF5ZXJJdGVtcxDjCxIU",
            "Cg9DMlNfUGxheWVySXRlbXMQ5AsSHAoXTUlfQnJvYWRjYXN0Q3JlYXRlV29y",
            "bGQQ0Q8SIQocTUlfQnJvYWRjYXN0Q3JlYXRlV29ybGRQcm94eRDSDxIZChRN",
            "SV9Xb3JsZFN5bmNUb0dhdGhlchC5FxIeChlNSV9Xb3JsZFByb3h5U3luY1Rv",
            "R2F0aGVyELoXEiAKG01JX0FjY291bnRTeW5jT25saW5lVG9SZWRpcxChHxIi",
            "Ch1NSV9BY2NvdW50RGVsZXRlT25saW5lVG9SZWRpcxCiHxIfChpNSV9QbGF5",
            "ZXJTeW5jT25saW5lVG9SZWRpcxCjHxIhChxNSV9QbGF5ZXJEZWxldGVPbmxp",
            "bmVUb1JlZGlzEKQfEhYKEU1JX1JvYm90U3luY1N0YXRlEIknEhMKDk1JX1Jv",
            "Ym90Q3JlYXRlEIonEhEKDE1JX0h0dHBCZWdpbhCQThIZChRNSV9IdHRwSW5u",
            "ZXJSZXNwb25zZRCRThIWChFNSV9IdHRwUmVxdWVzdEJhZBCSThIYChNNSV9I",
            "dHRwUmVxdWVzdExvZ2luEJNOEg8KCk1JX0h0dHBFbmQQg1ISGAoTTUlfSHR0",
            "cE91dGVyUmVxdWVzdBCEUhIZChRNSV9IdHRwT3V0ZXJSZXNwb25zZRCFUhIS",
            "CgxNSV9DbWRUaHJlYWQQoZwBEg8KCU1JX0NtZEFwcBCinAESFgoQTUlfQ21k",
            "V29ybGRQcm94eRCjnAESEQoLTUlfQ21kV29ybGQQpJwBEhIKDE1JX0NtZENy",
            "ZWF0ZRClnAESFgoQTUlfQ21kRWZmaWNpZW5jeRCmnAESEwoNTUlfRWZmaWNp",
            "ZW5jeRCnnAESFwoRQzJTX1N5bmNQbGF5ZXJQb3MQseoBEhcKEVMyQ19TeW5j",
            "UGxheWVyUG9zELLqARIXChFDMlNfU3luY1BsYXllckNtZBCz6gESFwoRUzJD",
            "X1N5bmNQbGF5ZXJDbWQQtOoBEhQKDkMyU19TeW5jTnBjUG9zELXqARIUCg5T",
            "MkNfU3luY05wY1BvcxC26gESGgoUUzJDX1N5bmNFbnRpdHlTdGF0dXMQt+oB",
            "EhYKEFMyQ19TeW5jRnNtU3RhdGUQueoBEhYKEEMyU19TeW5jRnNtU3RhdGUQ",
            "uuoBEhQKDkMyU19SZXFTeW5jTnBjELvqARIUCg5TMkNfUmVxU3luY05wYxC8",
            "6gESFwoRUzJDX1JlcUxpbmtQbGF5ZXIQveoBEhgKEkMyU19QbGF5ZXJBdGtF",
            "dmVudBC+6gESGAoSUzJDX1BsYXllckF0a0V2ZW50EL/qARIVCg9DMlNfTnBj",
            "QXRrRXZlbnQQwOoBEhUKD1MyQ19OcGNBdGtFdmVudBDB6gESFgoQUzJDX0Ry",
            "b3BJdGVtTGlzdBDC6gESGAoSQzJTX1VwZGF0ZUtuYXBJdGVtEMG4AhIXChFD",
            "MlNfR2V0UGxheWVyS25hcBDCuAISFwoRUzJDX0dldFBsYXllcktuYXAQw7gC",
            "YgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Proto.MsgId), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum MsgId {
    /// <summary>
    /// proto3的枚举，第一个必为0
    /// </summary>
    [pbr::OriginalName("None")] None = 0,
    /// <summary>
    /// 请求连接
    /// </summary>
    [pbr::OriginalName("MI_NetworkConnect")] MiNetworkConnect = 1,
    /// <summary>
    /// 连接成功
    /// </summary>
    [pbr::OriginalName("MI_NetworkConnected")] MiNetworkConnected = 2,
    /// <summary>
    /// 网络监听到一个连接
    /// </summary>
    [pbr::OriginalName("MI_NetworkListen")] MiNetworkListen = 3,
    /// <summary>
    /// 对网络监听进行一个Key值描述
    /// </summary>
    [pbr::OriginalName("MI_NetworkListenKey")] MiNetworkListenKey = 4,
    /// <summary>
    ///
    /// 物理网络的断开，network通知上层网络关闭事件
    /// 关闭可能是监听，可能是连接，根据协议体数据而定
    /// </summary>
    [pbr::OriginalName("MI_NetworkDisconnect")] MiNetworkDisconnect = 10,
    [pbr::OriginalName("MI_NetworkDisconnectEx")] MiNetworkDisconnectEx = 11,
    /// <summary>
    ///
    /// 逻辑层的断开，上层逻辑发起的主动断开网络的协议 to network
    /// 该协议没有协议体
    /// </summary>
    [pbr::OriginalName("MI_NetworkRequestDisconnect")] MiNetworkRequestDisconnect = 20,
    /// <summary>
    /// remove Component
    /// </summary>
    [pbr::OriginalName("MI_RemoveComponent")] MiRemoveComponent = 50,
    /// <summary>
    /// 创建Component
    /// </summary>
    [pbr::OriginalName("MI_CreateComponent")] MiCreateComponent = 51,
    /// <summary>
    /// 创建System
    /// </summary>
    [pbr::OriginalName("MI_CreateSystem")] MiCreateSystem = 52,
    [pbr::OriginalName("MI_Ping")] MiPing = 101,
    /// <summary>
    /// app 注册到 mgr
    /// </summary>
    [pbr::OriginalName("MI_AppRegister")] MiAppRegister = 102,
    /// <summary>
    /// app 数据同步
    /// </summary>
    [pbr::OriginalName("MI_AppInfoSync")] MiAppInfoSync = 103,
    /// <summary>
    /// app 数据同步
    /// </summary>
    [pbr::OriginalName("MI_AppInfoListSync")] MiAppInfoListSync = 104,
    /// <summary>
    /// login
    /// </summary>
    [pbr::OriginalName("C2L_AccountCheck")] C2LAccountCheck = 1000,
    /// <summary>
    ///   AccountCheckRs to client
    /// </summary>
    [pbr::OriginalName("C2L_AccountCheckRs")] C2LAccountCheckRs = 1001,
    /// <summary>
    /// 2.验证账号：验证账号之前，向Redis询问是已在其他的Login登录验证账号
    /// </summary>
    [pbr::OriginalName("MI_AccountQueryOnlineToRedis")] MiAccountQueryOnlineToRedis = 1002,
    [pbr::OriginalName("MI_AccountQueryOnlineToRedisRs")] MiAccountQueryOnlineToRedisRs = 1003,
    /// <summary>
    /// 1.选择角色：PlayerList 
    /// </summary>
    [pbr::OriginalName("L2DB_QueryPlayerList")] L2DbQueryPlayerList = 1010,
    /// <summary>
    ///   
    /// </summary>
    [pbr::OriginalName("L2DB_QueryPlayerListRs")] L2DbQueryPlayerListRs = 1011,
    /// <summary>
    /// </summary>
    [pbr::OriginalName("L2C_PlayerList")] L2CPlayerList = 1012,
    /// <summary>
    /// 2.选择角色：没有角色，请求创建角色
    /// </summary>
    [pbr::OriginalName("C2L_CreatePlayer")] C2LCreatePlayer = 1014,
    /// <summary>
    ///   CreatePlayerRs to client
    /// </summary>
    [pbr::OriginalName("C2L_CreatePlayerRs")] C2LCreatePlayerRs = 1015,
    /// <summary>
    /// 3.选择角色：创建角色ToDB
    /// </summary>
    [pbr::OriginalName("L2DB_CreatePlayer")] L2DbCreatePlayer = 1016,
    /// <summary>
    ///   CreatePlayerToDB
    /// </summary>
    [pbr::OriginalName("L2DB_CreatePlayerRs")] L2DbCreatePlayerRs = 1017,
    /// <summary>
    /// 4.选择角色 SelectPlayer
    /// </summary>
    [pbr::OriginalName("C2L_SelectPlayer")] C2LSelectPlayer = 1018,
    /// <summary>
    /// 
    /// </summary>
    [pbr::OriginalName("C2L_SelectPlayerRs")] C2LSelectPlayerRs = 1019,
    /// <summary>
    /// 1.请求登录的Token
    /// </summary>
    [pbr::OriginalName("MI_LoginTokenToRedis")] MiLoginTokenToRedis = 1022,
    /// <summary>
    ///   GameRequestTokenToRedisRs
    /// </summary>
    [pbr::OriginalName("MI_LoginTokenToRedisRs")] MiLoginTokenToRedisRs = 1023,
    /// <summary>
    /// 2.将token和Game发送给客户端，GameToken
    /// </summary>
    [pbr::OriginalName("L2C_GameToken")] L2CGameToken = 1024,
    /// <summary>
    /// game
    /// </summary>
    [pbr::OriginalName("C2G_LoginByToken")] C2GLoginByToken = 1100,
    /// <summary>
    ///   LoginByToken
    /// </summary>
    [pbr::OriginalName("C2G_LoginByTokenRs")] C2GLoginByTokenRs = 1101,
    /// <summary>
    /// 2.登录：Game请求登录的Token
    /// </summary>
    [pbr::OriginalName("MI_GameTokenToRedis")] MiGameTokenToRedis = 1102,
    /// <summary>
    ///   ameRequestTokenToRedis
    /// </summary>
    [pbr::OriginalName("MI_GameTokenToRedisRs")] MiGameTokenToRedisRs = 1103,
    /// <summary>
    /// 
    /// </summary>
    [pbr::OriginalName("G2DB_QueryPlayer")] G2DbQueryPlayer = 1104,
    /// <summary>
    /// 
    /// </summary>
    [pbr::OriginalName("G2DB_QueryPlayerRs")] G2DbQueryPlayerRs = 1105,
    /// <summary>
    /// 玩家初次整体同步数据
    /// </summary>
    [pbr::OriginalName("G2C_SyncPlayer")] G2CSyncPlayer = 1106,
    /// <summary>
    /// 进入地图：如果本地没有实例，向AppMgr请求
    /// </summary>
    [pbr::OriginalName("G2M_RequestWorld")] G2MRequestWorld = 1108,
    /// <summary>
    /// 进入地图，请求创建地图
    /// </summary>
    [pbr::OriginalName("G2S_CreateWorld")] G2SCreateWorld = 1109,
    /// <summary>
    /// 进入地图：开始跳转，player 设为 只读
    /// </summary>
    [pbr::OriginalName("MI_Teleport")] MiTeleport = 1110,
    /// <summary>
    ///   TeleportAfter 如果跳转失败，恢复本地player，如果成功，删除本地数据
    /// </summary>
    [pbr::OriginalName("MI_TeleportAfter")] MiTeleportAfter = 1111,
    /// <summary>
    /// 同步数据到Space
    /// </summary>
    [pbr::OriginalName("G2S_SyncPlayer")] G2SSyncPlayer = 1112,
    /// <summary>
    /// 请求同步数据
    /// </summary>
    [pbr::OriginalName("G2S_RequestSyncPlayer")] G2SRequestSyncPlayer = 1113,
    [pbr::OriginalName("G2DB_SavePlayer")] G2DbSavePlayer = 1114,
    [pbr::OriginalName("G2S_RemovePlayer")] G2SRemovePlayer = 1115,
    /// <summary>
    /// 进入地图，玩家请求进入某地图
    /// </summary>
    [pbr::OriginalName("C2G_EnterWorld")] C2GEnterWorld = 1116,
    /// <summary>
    /// 进入地图：如果没有实例，向AppMgr请求
    /// </summary>
    [pbr::OriginalName("G2M_QueryWorld")] G2MQueryWorld = 1117,
    [pbr::OriginalName("G2M_QueryWorldRs")] G2MQueryWorldRs = 1118,
    /// <summary>
    /// space
    /// </summary>
    [pbr::OriginalName("S2C_EnterWorld")] S2CEnterWorld = 1501,
    /// <summary>
    /// 同步数据给Game    	
    /// </summary>
    [pbr::OriginalName("S2G_SyncPlayer")] S2GSyncPlayer = 1502,
    /// <summary>
    /// 玩家出现
    /// </summary>
    [pbr::OriginalName("S2C_AllRoleAppear")] S2CAllRoleAppear = 1503,
    /// <summary>
    /// 玩家消失
    /// </summary>
    [pbr::OriginalName("S2C_RoleDisappear")] S2CRoleDisappear = 1504,
    /// <summary>
    /// 玩家移动
    /// </summary>
    [pbr::OriginalName("C2S_Move")] C2SMove = 1505,
    [pbr::OriginalName("S2C_Move")] S2CMove = 1506,
    [pbr::OriginalName("S2C_PlayerItems")] S2CPlayerItems = 1507,
    [pbr::OriginalName("C2S_PlayerItems")] C2SPlayerItems = 1508,
    /// <summary>
    /// 进入地图，创建地图广播
    /// </summary>
    [pbr::OriginalName("MI_BroadcastCreateWorld")] MiBroadcastCreateWorld = 2001,
    /// <summary>
    /// 进入地图，创建地图Proxy广播
    /// </summary>
    [pbr::OriginalName("MI_BroadcastCreateWorldProxy")] MiBroadcastCreateWorldProxy = 2002,
    /// <summary>
    /// Sync
    /// </summary>
    [pbr::OriginalName("MI_WorldSyncToGather")] MiWorldSyncToGather = 3001,
    /// <summary>
    /// world proxy
    /// </summary>
    [pbr::OriginalName("MI_WorldProxySyncToGather")] MiWorldProxySyncToGather = 3002,
    /// <summary>
    /// redis
    /// </summary>
    [pbr::OriginalName("MI_AccountSyncOnlineToRedis")] MiAccountSyncOnlineToRedis = 4001,
    /// <summary>
    /// 删除 account online 标志
    /// </summary>
    [pbr::OriginalName("MI_AccountDeleteOnlineToRedis")] MiAccountDeleteOnlineToRedis = 4002,
    /// <summary>
    /// 设置 account online 标志
    /// </summary>
    [pbr::OriginalName("MI_PlayerSyncOnlineToRedis")] MiPlayerSyncOnlineToRedis = 4003,
    /// <summary>
    /// 删除 account online 标志
    /// </summary>
    [pbr::OriginalName("MI_PlayerDeleteOnlineToRedis")] MiPlayerDeleteOnlineToRedis = 4004,
    /// <summary>
    /// robot
    /// </summary>
    [pbr::OriginalName("MI_RobotSyncState")] MiRobotSyncState = 5001,
    /// <summary>
    /// 请求创建Robot
    /// </summary>
    [pbr::OriginalName("MI_RobotCreate")] MiRobotCreate = 5002,
    /// <summary>
    /// http listen 的请求（外部请求）
    /// </summary>
    [pbr::OriginalName("MI_HttpBegin")] MiHttpBegin = 10000,
    /// <summary>
    /// 响应数据
    /// </summary>
    [pbr::OriginalName("MI_HttpInnerResponse")] MiHttpInnerResponse = 10001,
    [pbr::OriginalName("MI_HttpRequestBad")] MiHttpRequestBad = 10002,
    [pbr::OriginalName("MI_HttpRequestLogin")] MiHttpRequestLogin = 10003,
    [pbr::OriginalName("MI_HttpEnd")] MiHttpEnd = 10499,
    /// <summary>
    /// http connector 的消息（内部请求，外部返回）
    /// </summary>
    [pbr::OriginalName("MI_HttpOuterRequest")] MiHttpOuterRequest = 10500,
    /// <summary>
    /// 外部响应数据
    /// </summary>
    [pbr::OriginalName("MI_HttpOuterResponse")] MiHttpOuterResponse = 10501,
    /// <summary>
    /// cmd
    /// </summary>
    [pbr::OriginalName("MI_CmdThread")] MiCmdThread = 20001,
    [pbr::OriginalName("MI_CmdApp")] MiCmdApp = 20002,
    [pbr::OriginalName("MI_CmdWorldProxy")] MiCmdWorldProxy = 20003,
    [pbr::OriginalName("MI_CmdWorld")] MiCmdWorld = 20004,
    /// <summary>
    /// appmgr: the infomation of create
    /// </summary>
    [pbr::OriginalName("MI_CmdCreate")] MiCmdCreate = 20005,
    [pbr::OriginalName("MI_CmdEfficiency")] MiCmdEfficiency = 20006,
    [pbr::OriginalName("MI_Efficiency")] MiEfficiency = 20007,
    /// <summary>
    /// AI
    /// </summary>
    [pbr::OriginalName("C2S_SyncPlayerPos")] C2SSyncPlayerPos = 30001,
    [pbr::OriginalName("S2C_SyncPlayerPos")] S2CSyncPlayerPos = 30002,
    [pbr::OriginalName("C2S_SyncPlayerCmd")] C2SSyncPlayerCmd = 30003,
    [pbr::OriginalName("S2C_SyncPlayerCmd")] S2CSyncPlayerCmd = 30004,
    [pbr::OriginalName("C2S_SyncNpcPos")] C2SSyncNpcPos = 30005,
    [pbr::OriginalName("S2C_SyncNpcPos")] S2CSyncNpcPos = 30006,
    [pbr::OriginalName("S2C_SyncEntityStatus")] S2CSyncEntityStatus = 30007,
    [pbr::OriginalName("S2C_SyncFsmState")] S2CSyncFsmState = 30009,
    [pbr::OriginalName("C2S_SyncFsmState")] C2SSyncFsmState = 30010,
    [pbr::OriginalName("C2S_ReqSyncNpc")] C2SReqSyncNpc = 30011,
    [pbr::OriginalName("S2C_ReqSyncNpc")] S2CReqSyncNpc = 30012,
    [pbr::OriginalName("S2C_ReqLinkPlayer")] S2CReqLinkPlayer = 30013,
    [pbr::OriginalName("C2S_PlayerAtkEvent")] C2SPlayerAtkEvent = 30014,
    [pbr::OriginalName("S2C_PlayerAtkEvent")] S2CPlayerAtkEvent = 30015,
    [pbr::OriginalName("C2S_NpcAtkEvent")] C2SNpcAtkEvent = 30016,
    [pbr::OriginalName("S2C_NpcAtkEvent")] S2CNpcAtkEvent = 30017,
    [pbr::OriginalName("S2C_DropItemList")] S2CDropItemList = 30018,
    [pbr::OriginalName("C2S_UpdateKnapItem")] C2SUpdateKnapItem = 40001,
    [pbr::OriginalName("C2S_GetPlayerKnap")] C2SGetPlayerKnap = 40002,
    [pbr::OriginalName("S2C_GetPlayerKnap")] S2CGetPlayerKnap = 40003,
  }

  #endregion

}

#endregion Designer generated code
