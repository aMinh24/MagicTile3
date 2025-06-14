public class BaseNotify : BaseUIElement
{
    public override void Hide()
    {
        base.Hide();
        uiType = UIType.Notify;
    }

    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
    }
}
