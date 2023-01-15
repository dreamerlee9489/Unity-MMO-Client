using Manage;

namespace UI
{
    public class TradePanel : BasePanel
    {
        private TradeRect _tradeRectA, _tradeRectR, _localRect, _remoteRect;

        protected override void Awake()
        {
            base.Awake();
            _tradeRectA = transform.Find("TradeRectA").GetComponent<TradeRect>();
            _tradeRectR = transform.Find("TradeRectR").GetComponent<TradeRect>();
            Close();
        }

        public TradeRect LocalRect => _localRect;
        public TradeRect RemoteRect => _remoteRect;

        public void Open(Proto.AppearRole applicant, Proto.AppearRole responder)
        {
            if (GameManager.Instance.mainPlayer.Sn == applicant.sn)
            {
                _tradeRectA.SetLocalRect(applicant.name);
                _tradeRectR.SetRemoteRect(responder.name);
                _localRect = _tradeRectA;
                _remoteRect = _tradeRectR;
            }
            else
            {
                _tradeRectA.SetRemoteRect(applicant.name);
                _tradeRectR.SetLocalRect(responder.name);
                _localRect = _tradeRectR;
                _remoteRect = _tradeRectA;
            }
            gameObject.SetActive(true);
        }
    }
}
