using Manage;

namespace UI
{
    public class TradePanel : BasePanel
    {
        private TradeRect _tradeRectA, _tradeRectR;

        public ulong localSn, remoteSn;

        public TradeRect LocalRect { get; set; }
        public TradeRect RemoteRect { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _tradeRectA = transform.Find("TradeRectA").GetComponent<TradeRect>();
            _tradeRectR = transform.Find("TradeRectR").GetComponent<TradeRect>();
            Close();
        }

        public void Open(Proto.AppearRole applicant, Proto.AppearRole responder)
        {
            if (GameManager.Instance.mainPlayer.Sn == applicant.sn)
            {
                localSn = applicant.sn;
                remoteSn = responder.sn;
                _tradeRectA.SetLocalRect(applicant);
                _tradeRectR.SetRemoteRect(responder);
                LocalRect = _tradeRectA;
                RemoteRect = _tradeRectR;
            }
            else
            {
                localSn = responder.sn;
                remoteSn = applicant.sn;
                _tradeRectA.SetRemoteRect(applicant);
                _tradeRectR.SetLocalRect(responder);
                LocalRect = _tradeRectR;
                RemoteRect = _tradeRectA;
            }
            LocalRect.GoldField.onEndEdit.AddListener((str) =>
            {
                if(str.Length > 0)
                {
                    Proto.UpdateTradeItem proto = new()
                    {
                        Sender = UIManager.Instance.GetPanel<TradePanel>().localSn,
                        Recver = UIManager.Instance.GetPanel<TradePanel>().remoteSn,
                        Ack = false,
                        Gold = int.Parse(str),
                        Item = null
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2CUpdateTradeItem, proto);
                }
            });
            LocalRect.SureTog.onValueChanged.AddListener((isOn) =>
            {
                Proto.UpdateTradeItem proto = new()
                {
                    Sender = UIManager.Instance.GetPanel<TradePanel>().localSn,
                    Recver = UIManager.Instance.GetPanel<TradePanel>().remoteSn,
                    Ack = isOn,
                    Gold = 0,
                    Item = null
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CUpdateTradeItem, proto);
            });
            gameObject.SetActive(true);
        }

        public override void Close()
        {
            if (LocalRect != null)
            {
                LocalRect.GoldField.onEndEdit.RemoveAllListeners();
                LocalRect.SureTog.onValueChanged.RemoveAllListeners();
            }
            base.Close();
        }
    }
}
