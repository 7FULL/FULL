using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager: MonoBehaviour
{
    [SerializeField]
    private List<MenuStruct> menus;
    
    private MenuStruct currentMenu;
    
    // Singleton
    public static MenuManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void OpenMenu(Menu menu, bool mouseFocus = false)
    {
        foreach (MenuStruct menuStruct in menus)
        {
            if (menuStruct.MenuType == menu)
            {
                MenuUtils iMenu = menuStruct.MenuGameObject.GetComponent<MenuUtils>();
                
                if (iMenu == null || !iMenu.HasAnimation)
                {
                    menuStruct.MenuGameObject.SetActive(true);
                }
                else
                {
                    iMenu.OpenAnimation();
                }
                currentMenu = menuStruct;
            }
            else
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
        }
        
        // Free the mouse
        if (!mouseFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
            
            // Free the mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }   
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