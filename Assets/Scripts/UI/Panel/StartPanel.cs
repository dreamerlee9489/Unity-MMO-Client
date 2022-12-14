namespace UI
{
    public class StartPanel : BasePanel
    {
        LoginPanel _loginPanel = null;
        RolesPanel _rolesPanel = null;

        protected override void Awake()
        {
            base.Awake();
            _panelType = PanelType.StartPanel;
            _loginPanel = transform.Find("LoginPanel").GetComponent<LoginPanel>();
            _rolesPanel = transform.Find("RolesPanel").GetComponent<RolesPanel>();
        }

        protected override void Start()
        {
            base.Start();
        }
    }
}
