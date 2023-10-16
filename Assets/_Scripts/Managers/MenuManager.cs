using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager: MonoBehaviour
{
    [SerializeField]
    private List<MenuStruct> _menus;
    
    private MenuStruct _currentMenu;
    
    // Singleton
    public static MenuManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void OpenMenu(Menu menu)
    {
        foreach (MenuStruct menuStruct in _menus)
        {
            if (menuStruct.MenuType == menu)
            {
                MenuUtils iMenu = menuStruct.Menu.GetComponent<MenuUtils>();
                
                if (iMenu == null || !iMenu.HasAnimation)
                {
                    menuStruct.Menu.SetActive(true);
                }
                else
                {
                    iMenu.OpenAnimation();
                }
                _currentMenu = menuStruct;
                
                // Free the mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                menuStruct.Menu.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // If ther is no menu and player clicks the mouse button, focus cursor on the game
        if (_currentMenu.Menu != null && !_currentMenu.Menu.activeSelf && Input.GetMouseButtonDown(0))
        {
            ToGame();
        }
    }

    public void CloseMenu()
    {
        if (_currentMenu.Menu != null)
        {
            MenuUtils iMenu = _currentMenu.Menu.GetComponent<MenuUtils>();
            
            if (iMenu == null || !iMenu.HasAnimation)
            {
                _currentMenu.Menu.SetActive(false);
            }
            else
            {
                iMenu.CloseAnimation();
            }
            
            _currentMenu = new MenuStruct(null, Menu.NONE);
            
            // Free the mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }   
    }
    
    public void RegisterMenu(MenuStruct menuStruct)
    {
        _menus.Add(menuStruct);
    }
    
    public void RegisterMenu(GameObject menu, Menu menuType)
    {
        _menus.Add(new MenuStruct(menu, menuType));
    }
    
    public void RegisterMenu(MenuStruct[] menuStructs)
    {
        foreach (MenuStruct menuStruct in menuStructs)
        {
            _menus.Add(menuStruct);
        }
    }
    
    /// <summary>
    /// This function closes all menus and focus cursor on the game
    /// </summary>
    public void ToGame()
    {
        foreach (MenuStruct menuStruct in _menus)
        {
            if (menuStruct.MenuType != Menu.NONE)
            {
                MenuUtils iMenu = menuStruct.Menu.GetComponent<MenuUtils>();
                
                if (iMenu == null || !iMenu.HasAnimation)
                {
                    menuStruct.Menu.SetActive(false);
                }
                else
                {
                    iMenu.CloseAnimation();
                }
            }
        }
        
        _currentMenu = new MenuStruct(null, Menu.NONE);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

[Serializable]
public struct MenuStruct
{
    [Tooltip("The menu game object")]
    [InspectorName("Menu")]
    [SerializeField]
    private GameObject _menu;
    
    [Tooltip("The menu type")]
    [InspectorName("Menu Type")]
    [SerializeField]
    private Menu _menuType;
    
    public GameObject Menu => _menu;
    public Menu MenuType => _menuType;
    
    public MenuStruct(GameObject menu, Menu menuType)
    {
        _menu = menu;
        _menuType = menuType;
    }
}