using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    Unknow,
    Screen,
    Popup,
    Notify,
    Overlap
}

// UIManager now acts as a Facade
public class UIManager : BaseManager<UIManager>
{
    public GameObject cScreen, cPopup, cNotify, cOverlap; // Keep references for subsystems

    // Subsystem instances
    private ScreenSubsystem screenSubsystem;
    private PopupSubsystem popupSubsystem;
    private NotifySubsystem notifySubsystem;
    private OverlapSubsystem overlapSubsystem;

    // Public accessors for subsystems (optional, depends on design needs)
    public ScreenSubsystem Screens => screenSubsystem;
    public PopupSubsystem Popups => popupSubsystem;
    public NotifySubsystem Notifies => notifySubsystem;
    public OverlapSubsystem Overlaps => overlapSubsystem;

    // Public accessors for current elements via subsystems
    public BaseScreen CurScreen => screenSubsystem?.CurrentElement;
    public BasePopup CurPopup => popupSubsystem?.CurrentElement;
    public BaseNotify CurNotify => notifySubsystem?.CurrentElement;
    public BaseOverlap CurOverlap => overlapSubsystem?.CurrentElement;

    private const string SCREEN_RESOURCES_PATH = "Prefabs/UI/Screen/";
    private const string POPUP_RESOURCES_PATH = "Prefabs/UI/Popup/";
    private const string NOTIFY_RESOURCES_PATH = "Prefabs/UI/Notify/";
    private const string OVERLAP_RESOURCES_PATH = "Prefabs/UI/Overlap/";

    protected override void Awake() // Initialize subsystems in Awake
    {
        base.Awake(); // Call base Awake if it exists in BaseManager
        InitializeSubsystems();
    }

    private void InitializeSubsystems()
    {
        screenSubsystem = new ScreenSubsystem(this);
        popupSubsystem = new PopupSubsystem(this);
        notifySubsystem = new NotifySubsystem(this);
        overlapSubsystem = new OverlapSubsystem(this);
    }

    #region Generic Helpers (Internal Implementation for Subsystems)

    internal Transform GetParentTransform(UIType type) // Changed to internal
    {
        switch (type)
        {
            case UIType.Screen: return cScreen.transform;
            case UIType.Popup: return cPopup.transform;
            case UIType.Notify: return cNotify.transform;
            case UIType.Overlap: return cOverlap.transform;
            default:
                Debug.LogError($"Unsupported UIType for parent: {type}");
                return null;
        }
    }

    internal T GetNewUI<T>(UIType uiType) where T : MonoBehaviour // Changed to internal
    {
        string uiName = typeof(T).Name;
        GameObject pfUI = GetUIPrefab(uiType, uiName);
        if (pfUI == null)
        {
            throw new MissingReferenceException($"Cannot find prefab for {uiName} ({uiType}). Path: {GetUIPrefabPath(uiType, uiName)}");
        }

        T uiComponent = pfUI.GetComponent<T>();
        if (uiComponent == null)
        {
            throw new MissingComponentException($"Prefab for {uiName} ({uiType}) does not have the required component {typeof(T).FullName}.");
        }

        Transform parent = GetParentTransform(uiType);
        if (parent == null) return null;

        GameObject ob = Instantiate(pfUI, parent) as GameObject;
        ob.transform.localScale = Vector3.one;
        ob.transform.localPosition = Vector3.zero;
        ob.transform.localRotation = Quaternion.identity;
        ob.SetActive(false);

#if UNITY_EDITOR
        ob.name = $"{uiType.ToString().ToUpper()}_{uiName}";
#endif

        T instance = ob.GetComponent<T>();
        BaseUIElement uiElementInstance = instance as BaseUIElement;
        if (uiElementInstance != null)
        {
            uiElementInstance.Init();
        }
        else
        {
            Debug.LogError($"Instantiated UI {typeof(T).Name} does not inherit from BaseUIElement. Cannot call Init().", instance);
        }
        return instance;
    }

    internal void RemoveUI<T>(string name, Dictionary<string, T> collection) where T : MonoBehaviour // Changed to internal
    {
        if (collection.TryGetValue(name, out T uiElement))
        {
            if (uiElement != null && uiElement.gameObject != null)
            {
                Destroy(uiElement.gameObject);
            }
            collection.Remove(name);
        }
    }

    internal void HideAllUI<T>(Dictionary<string, T> collection) where T : MonoBehaviour // Changed to internal
    {
        List<string> keys = new List<string>(collection.Keys);
        foreach (string key in keys)
        {
            if (collection.TryGetValue(key, out T uiElementComponent))
            {
                if (uiElementComponent != null)
                {
                    BaseUIElement element = uiElementComponent as BaseUIElement;
                    if (element != null && !element.IsHide)
                    {
                        element.Hide();
                    }
                }
            }
        }
    }

    internal U GetExistUI<T, U>(Dictionary<string, U> collection) where T : U where U : MonoBehaviour // Changed to internal
    {
        string uiName = typeof(T).Name;
        if (collection.TryGetValue(uiName, out U uiElement))
        {
            return uiElement;
        }
        return null;
    }

    internal U ShowUIElementLogic<T, U>( // Changed to internal
        object data,
        bool forceShowData,
        Dictionary<string, U> collection,
        U currentElement, // Passed from the subsystem
        Func<U> createNewElement, // Passed from the subsystem (e.g., subsystem.GetNew<T>)
        Action<string> removeElementAction // Passed from the subsystem (e.g., subsystem.Remove)
        ) where T : U where U : MonoBehaviour
    {
        string elementName = typeof(T).Name;
        U result = null;
        U newCurrentElement = currentElement;

        if (currentElement != null)
        {
            var currentName = currentElement.GetType().Name;
            if (currentName.Equals(elementName))
            {
                result = currentElement;
            }
            else
            {
                (currentElement as BaseUIElement)?.Hide();
                removeElementAction(currentName); // Call subsystem's remove action
                newCurrentElement = null;
            }
        }

        if (result == null)
        {
            if (!collection.TryGetValue(elementName, out result))
            {
                result = createNewElement(); // Call subsystem's create action
                if (result != null)
                {
                    collection.Add(elementName, result);
                }
                else
                {
                    Debug.LogError($"Failed to create new UI element: {elementName}");
                    return newCurrentElement; // Return original current element on failure
                }
            }
        }

        bool isShow = false;
        if (result != null)
        {
            BaseUIElement resultElement = result as BaseUIElement;
            if (resultElement != null && (forceShowData || resultElement.IsHide))
            {
                isShow = true;
            }
        }

        if (isShow && result != null)
        {
            newCurrentElement = result; // Update the potential new current element
            result.transform.SetAsLastSibling();
            (result as BaseUIElement)?.Show(data);
            result.gameObject.SetActive(true); // Ensure game object is active
        }
        else if (result != null && !isShow)
        {
            if (currentElement != null && currentElement.GetType().Name.Equals(elementName))
            {
                newCurrentElement = currentElement;
            }
        }

        return newCurrentElement; // Return the element that should now be current
    }

    private string GetUIPrefabPath(UIType t, string uiName)
    {
        switch (t)
        {
            case UIType.Screen: return SCREEN_RESOURCES_PATH + uiName;
            case UIType.Popup: return POPUP_RESOURCES_PATH + uiName;
            case UIType.Notify: return NOTIFY_RESOURCES_PATH + uiName;
            case UIType.Overlap: return OVERLAP_RESOURCES_PATH + uiName;
            default: return string.Empty;
        }
    }

    private GameObject GetUIPrefab(UIType t, string uiName)
    {
        string defaultPath = GetUIPrefabPath(t, uiName);
        if (string.IsNullOrEmpty(defaultPath))
        {
            Debug.LogError($"Invalid UIType specified: {t}");
            return null;
        }

        GameObject result = Resources.Load(defaultPath) as GameObject;
        return result;
    }

    #endregion

    #region Facade Methods (Delegating to Subsystems)

    // --- Screen Facade Methods ---
    public void HideAllScreens() => screenSubsystem?.HideAll();
    public T GetExistScreen<T>() where T : BaseScreen => screenSubsystem?.GetExist<T>();
    public void ShowScreen<T>(object data = null, bool forceShowData = true) where T : BaseScreen => screenSubsystem?.Show<T>(data, forceShowData);

    // --- Popup Facade Methods ---
    public void HideAllPopups() => popupSubsystem?.HideAll();
    public T GetExistPopup<T>() where T : BasePopup => popupSubsystem?.GetExist<T>();
    public void ShowPopup<T>(object data = null, bool forceShowData = true) where T : BasePopup => popupSubsystem?.Show<T>(data, forceShowData);

    // --- Notify Facade Methods ---
    public void HideAllNotifies() => notifySubsystem?.HideAll();
    public T GetExistNotify<T>() where T : BaseNotify => notifySubsystem?.GetExist<T>();
    public void ShowNotify<T>(object data = null, bool forceShowData = false) where T : BaseNotify => notifySubsystem?.Show<T>(data, forceShowData);

    // --- Overlap Facade Methods ---
    public void HideAllOverlaps() => overlapSubsystem?.HideAll();
    public T GetExistOverlap<T>() where T : BaseOverlap => overlapSubsystem?.GetExist<T>();
    public void ShowOverlap<T>(object data = null, bool forceShowData = false) where T : BaseOverlap => overlapSubsystem?.Show<T>(data, forceShowData);

    // --- General UI Management Facade Methods ---
    public void HideAll(UIType type)
    {
        switch (type)
        {
            case UIType.Screen: HideAllScreens(); break;
            case UIType.Popup: HideAllPopups(); break;
            case UIType.Notify: HideAllNotifies(); break;
            case UIType.Overlap: HideAllOverlaps(); break;
            default: Debug.LogWarning($"HideAll called with unsupported UIType: {type}"); break;
        }
    }

    public void HideAll()
    {
        HideAllScreens();
        HideAllPopups();
        HideAllNotifies();
        HideAllOverlaps();
    }

    #endregion
}