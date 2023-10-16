using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// The class to create the dictionary in the inspector
[CustomEditor(typeof(ItemManager))]
public class DictionaryEditor : Editor
{
    // The name of the dictionary
    private string label = "Items";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("Si se quiere cambiar cualquier dato hacedlo de abajo a arriba", MessageType.Info);
        
        // Change for the target class
        ItemManager classToCustom = (ItemManager)target;

        if (classToCustom == null)
        {
            return;
        }
        
        List<Items> keysToRemove = new List<Items>();

        for (int i = 0; i < classToCustom.items.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            Items newKey = (Items)EditorGUILayout.EnumPopup(classToCustom.items.GetKey(i));
            Item newValue = (Item)EditorGUILayout.ObjectField(classToCustom.items.GetValue(i), typeof(Item), false);
            EditorGUILayout.EndHorizontal();

            if (newKey != classToCustom.items.GetKey(i))
            {
                keysToRemove.Add(classToCustom.items.GetKey(i));
                classToCustom.items.Add(newKey, newValue);
            }
            //Si el valor cambia en el editor lo cambiamos en el diccionario
            else if (newValue != classToCustom.items.GetValue(i))
            {
                //Creo que el problema de que se escriban los de abajo esta aqui pero ni ides
                classToCustom.items.SetValue(classToCustom.items.GetKey(i), newValue);
            }
        }

        // Remove keys that have changed.
        foreach (var key in keysToRemove)
        {
            classToCustom.items.Remove(key);
        }

        if (GUILayout.Button("Añadir"))
        {
            classToCustom.items.Add(Items.NONE,null);
        }

        if (GUILayout.Button("Eliminar"))
        {
            if (classToCustom.items.Count == 0)
            {
                return;
            }
            
            classToCustom.items.RemoveAt(classToCustom.items.Count - 1);
        }
    }
}