public class BaseScreen : BaseUIElement
{
    public override void Hide()
    {
        base.Hide();
        this.uiType = UIType.Screen;
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
