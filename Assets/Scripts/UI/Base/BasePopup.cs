public class BasePopup : BaseUIElement
{
    public override void Hide()
    {
        base.Hide();
        this.gameObject.SetActive(false);
        uiType = UIType.Popup;
    }

    public override void Init()
    {
        base.Init();
        this.gameObject.SetActive(true);

    }

    public override void Show(object data)
    {
        base.Show(data);
        this.gameObject.SetActive(true);

    }
}
