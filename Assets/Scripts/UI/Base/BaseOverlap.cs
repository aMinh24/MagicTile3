public class BaseOverlap : BaseUIElement
{
    public override void Hide()
    {
        base.Hide();
        uiType = UIType.Overlap;
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
