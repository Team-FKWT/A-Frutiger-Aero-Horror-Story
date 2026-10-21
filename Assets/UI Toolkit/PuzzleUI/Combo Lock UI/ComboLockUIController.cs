using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ComboLockUIController: MonoBehaviour
{
   // Get references to all lock elements
   private Button dial1Up;
   private Label dial1Number;
   private Button dial1Down;
   
   private Button dial2Up;
   private Label dial2Number;
   private Button dial2Down;
   
   private Button dial3Up;
   private Label dial3Number;
   private Button dial3Down;

   private Button shackle;
   
   // Supply correct combo in editor
   public string answer;
   
   void Awake()
   {
      GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
   }

   void OnDestroy()
   {
      GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
   }

   private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
   {
      // Get references to UI elements
      dial1Up = root.Q<Button>("Dial1Up");
      dial1Number = root.Q<Label>("Dial1");
      dial1Down = root.Q<Button>("Dial1Down");
      dial2Up = root.Q<Button>("Dial2Up");
      dial2Number = root.Q<Label>("Dial2");
      dial2Down = root.Q<Button>("Dial2Down");
      dial3Up = root.Q<Button>("Dial3Up");
      dial3Number = root.Q<Label>("Dial3");
      dial3Down = root.Q<Button>("Dial3Down");
      shackle = root.Q<Button>("LockShackle");
      
      // Make buttons update lock dials
      dial1Up.clicked += () =>
      {
         dial1Number.text = ((Int32.Parse(dial1Number.text) % 9) + 1).ToString();
      };
      dial1Down.clicked += () =>
      {
         dial1Number.text = ((Int32.Parse(dial1Number.text) + 7) % 9 + 1).ToString();
      };
      dial2Up.clicked += () =>
      {
         dial2Number.text = ((Int32.Parse(dial2Number.text) % 9) + 1).ToString();
      };
      dial2Down.clicked += () =>
      {
         dial2Number.text = ((Int32.Parse(dial2Number.text) + 7) % 9 + 1).ToString();
      };
      dial3Up.clicked += () =>
      {
         dial3Number.text = ((Int32.Parse(dial3Number.text) % 9) + 1).ToString();
      };
      dial3Down.clicked += () =>
      {
         dial3Number.text = ((Int32.Parse(dial3Number.text) + 7) % 9 + 1).ToString();
      };
      // Shackle checks answer when clicked
      shackle.clicked += () =>
      {
         // TODO update puzzle completion state in ProgressionState
         if (dial1Number.text + dial2Number.text + dial3Number.text == answer)
            gameObject.GetComponentInParent<ComboLockController>().DisengageInteract();
      };
   }
}