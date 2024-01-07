using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager: MonoBehaviour
{
    [SerializeField]
    private List<MenuStruct> menus;
    
    private MenuStruct currentMenu;
    
    private PopUp popup;
    
    private GameObject crossHair;
    
    // Singleton
    public static MenuManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    
    /// <summary>
    /// This function opens a menu and focus cursor on it
    /// </summary>
    /// <param name="menu">The menu to open</param>
    /// <param name="keepFocus">If true, the mouse will not be focused on the menu</param>
    public void OpenMenu(Menu menu, bool keepFocus = false)
    {
        if (currentMenu.MenuGameObject != null)
        {
            MenuUtils iMenu = currentMenu.MenuGameObject.GetComponent<MenuUtils>();
            
            iMenu.CloseAnimation();
        }
        
        foreach (MenuStruct menuStruct in menus)
        {
            if (menuStruct.MenuType == menu)
            {
                MenuUtils iMenu = menuStruct.MenuGameObject.GetComponent<MenuUtils>();
                
                menuStruct.MenuGameObject.SetActive(true);
                    
                iMenu.OpenAnimation();
                
                currentMenu = menuStruct;
            }
        }
        
        // Free the mouse
        if (!keepFocus)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            
            GameManager.Instance.Player.Stop();
        }
        
        //Hide the crosshair if the menu is not a call menu
        if (menu != Menu.CALL && menu != Menu.CALLING && menu != Menu.VIDEO_CALL && menu != Menu.RECEIVE_CALL && menu != Menu.CONTACT_REQUEST)
        {
            crossHair.SetActive(false);
        }
        else
        {
            crossHair.SetActive(true);
        }
    }

    private void Update()
    {
        // If ther is no menuGameObject and player clicks the mouse button, focus cursor on the game
        if (currentMenu.MenuGameObject != null && !currentMenu.MenuGameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            ToGame();
        }
    }

    public void CloseMenu()
    {
        if (currentMenu.MenuGameObject != null)
        {
            crossHair.SetActive(true);
            
            MenuUtils iMenu = currentMenu.MenuGameObject.GetComponent<MenuUtils>();
            
            if (iMenu == null || !iMenu.HasAnimation)
            {
                currentMenu.MenuGameObject.SetActive(false);
            }
            else
            {
                iMenu.CloseAnimation();
            }
            
            currentMenu = new MenuStruct(null, Menu.NONE, null);
            
            // Lock the mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            GameManager.Instance.Player.Resume();
        }   
    }
    
    public void SetPopUp(PopUp popup)
    {
        this.popup = popup;
    }
    
    public void SetCrossHair(GameObject crossHair)
    {
        this.crossHair = crossHair;
    }
    
    public void PopUp(string message)
    {
        CloseMenu();
        
        popup.Configure(message);
        popup.OpenAnimation();
        
        StartCoroutine(WaitToClosePopUp());
    }
    
    IEnumerator WaitToClosePopUp()
    {
        yield return new WaitForSecondsRealtime(3f);
        
        popup.CloseAnimation();
    }
    
    public void RegisterMenu(MenuStruct menuStruct)
    {
        menus.Add(menuStruct);
    }
    
    public void RegisterMenu(GameObject menu, Menu menuType, MenuUtils menuUtils)
    {
        menus.Add(new MenuStruct(menu, menuType, menuUtils));
    }
    
    public void RegisterMenu(MenuStruct[] menuStructs)
    {
        foreach (MenuStruct menuStruct in menuStructs)
        {
            menus.Add(menuStruct);
        }
    }
    
    /// <summary>
    /// This function closes all menus and focus cursor on the game
    /// </summary>
    public void ToGame()
    {
        foreach (MenuStruct menuStruct in menus)
        {
            if (menuStruct.MenuType != Menu.NONE)
            {
                MenuUtils iMenu = menuStruct.MenuGameObject.GetComponent<MenuUtils>();
                
                if (iMenu == null || !iMenu.HasAnimation)
                {
                    menuStruct.MenuGameObject.SetActive(false);
                }
                else
                {
                    iMenu.CloseAnimation();
                }
            }
        }
        
        currentMenu = new MenuStruct(null, Menu.NONE, null);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        GameManager.Instance.Player.Resume();
    }
    
    public MenuStruct GetMenu(Menu menu)
    {
        foreach (MenuStruct menuStruct in menus)
        {
            if (menuStruct.MenuType == menu)
            {
               return menuStruct;
            }
        }
        
        return new MenuStruct(null, Menu.NONE, null);
    }

    public void Focus()
    {
        // Focus mouse on the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        GameManager.Instance.Player.Resume();
    }
    
    public void ClearNulls()
    {
        for (int i = 0; i < menus.Count; i++)
        {
            if (menus[i].MenuGameObject == null)
            {
                menus.RemoveAt(i);
            }
        }
    }
}

[Serializable]
public struct MenuStruct
{
    [Tooltip("The _menuGameObject game object")]
    [InspectorName("MenuGameObject")]
    [SerializeField]
    private GameObject _menuGameObject;
    
    [Tooltip("The _menuGameObject type")]
    [InspectorName("MenuGameObject Type")]
    [SerializeField]
    private Menu _menuType;
    
    public GameObject MenuGameObject => _menuGameObject;
    public Menu MenuType => _menuType;
    
    private MenuUtils _menuUtils;
    
    public MenuUtils Menu => _menuUtils;
    
    public MenuStruct(GameObject menuGameObject, Menu menuType, MenuUtils menuUtils)
    {
        this._menuGameObject = menuGameObject;
        _menuType = menuType;
        _menuUtils = menuUtils;
    }
}