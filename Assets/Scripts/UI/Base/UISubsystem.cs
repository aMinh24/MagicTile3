using System.Collections.Generic;
using UnityEngine;

// --- Base Class (Optional but helps reduce repetition) ---
// Note: Using dynamic extensively here to keep BaseUI classes unchanged.
// Consider interfaces (e.g., IUIElement with Init, Show, Hide, IsHide) for better type safety if Base classes can be modified.
public abstract class UISubsystem<TBase> where TBase : MonoBehaviour
{
    protected UIManager uiManager; // Reference to the Facade
    protected Dictionary<string, TBase> collection = new Dictionary<string, TBase>();
    protected TBase currentElement;
    protected UIType uiType;

    public UISubsystem(UIManager manager, UIType type)
    {
        this.uiManager = manager;
        this.uiType = type;
    }

    public Dictionary<string, TBase> Collection => collection;
    public TBase CurrentElement => currentElement;

    protected TBase GetNew<T>() where T : TBase
    {
        return uiManager.GetNewUI<T>(uiType); // Call Facade's helper
    }

    public void HideAll()
    {
        uiManager.HideAllUI(collection); // Call Facade's helper
    }

    public T GetExist<T>() where T : TBase
    {
        // Cast is needed because GetExistUI returns the base type U (TBase here)
        return uiManager.GetExistUI<T, TBase>(collection) as T; // Call Facade's helper
    }

    protected void Remove(string name)
    {
        uiManager.RemoveUI(name, collection); // Call Facade's helper
    }

    public void Show<T>(object data, bool forceShowData) where T : TBase
    {
        // Call Facade's logic, passing subsystem-specific methods/data
        currentElement = uiManager.ShowUIElementLogic<T, TBase>(
            data,
            forceShowData,
            collection,
            currentElement,
            GetNew<T>, // Pass the subsystem's GetNew method
            Remove     // Pass the subsystem's Remove method
        );
    }
}

// --- Concrete Subsystem Classes ---

public class ScreenSubsystem : UISubsystem<BaseScreen>
{
    public ScreenSubsystem(UIManager manager) : base(manager, UIType.Screen) { }
    // Specific methods for Screens if needed in the future
}

public class PopupSubsystem : UISubsystem<BasePopup>
{
    public PopupSubsystem(UIManager manager) : base(manager, UIType.Popup) { }
    // Specific methods for Popups if needed in the future
}

public class NotifySubsystem : UISubsystem<BaseNotify>
{
    public NotifySubsystem(UIManager manager) : base(manager, UIType.Notify) { }
    // Specific methods for Notifies if needed in the future
}

public class OverlapSubsystem : UISubsystem<BaseOverlap>
{
    public OverlapSubsystem(UIManager manager) : base(manager, UIType.Overlap) { }

    // Example of overriding if needed, though base class handles it now
    // public new void Show<T>(object data, bool forceShowData) where T : BaseOverlap
    // {
    //     base.Show<T>(data, forceShowData);
    // }

    // Need to redefine GetNewOverLap if ShowUIElementLogic needs the specific name
    // Or adjust ShowUIElementLogic to accept the correct Func type
    public BaseOverlap GetNewOverLap<T>() where T : BaseOverlap // Keep specific name if needed elsewhere
    {
        return base.GetNew<T>();
    }
}
