using UnityEngine;
using UnityEngine.UIElements;

public class TabbedMenuController
{
    /* Define member variables*/
    private const string TabClassName = "tab";
    private const string CurrentlySelectedTabClassName = "currentlySelectedTab";
    private const string NonSelectedTabClassName = "nonSelectedTab";
    private const string UnselectedContentClassName = "hiddenContent";
    // Tab and tab content have the same prefix but different suffix
    // Define the suffix of the tab name
    private const string TabNameSuffix = "Tab";
    // Define the suffix of the tab content name
    private const string ContentNameSuffix = "Content";

    private readonly VisualElement root;

    public TabbedMenuController(VisualElement root)
    {
        this.root = root;
    }

    public void RegisterTabCallbacks()
    {
        UQueryBuilder<Label> tabs = GetAllTabs();
        tabs.ForEach((Label tab) => {
            tab.RegisterCallback<ClickEvent>(TabOnClick);
        });
    }

    /* Method for the tab on-click event: 

       - If it is not selected, find other tabs that are selected, unselect them 
       - Then select the tab that was clicked on
    */
    private void TabOnClick(ClickEvent evt)
    {
        Label clickedTab = evt.currentTarget as Label;
        if (!TabIsCurrentlySelected(clickedTab))
        {
            GetAllTabs().Where(
                (tab) => tab != clickedTab && TabIsCurrentlySelected(tab)
            ).ForEach(UnselectTab);
            SelectTab(clickedTab);
        }
    }
    //Method that returns a Boolean indicating whether a tab is currently selected
    private static bool TabIsCurrentlySelected(Label tab)
    {
        return tab.ClassListContains(CurrentlySelectedTabClassName);
    }

    private UQueryBuilder<Label> GetAllTabs()
    {
        return root.Query<Label>(className: TabClassName);
    }

    /* Method for the selected tab: 
       -  Takes a tab as a parameter and adds the currentlySelectedTab class
       -  Then finds the tab content and removes the unselectedContent class */
    private void SelectTab(Label tab)
    {
        tab.AddToClassList(CurrentlySelectedTabClassName);
        tab.RemoveFromClassList(NonSelectedTabClassName);
        VisualElement content = FindContent(tab);
        content.RemoveFromClassList(UnselectedContentClassName);
    }

    /* Method for the unselected tab: 
       -  Takes a tab as a parameter and removes the currentlySelectedTab class
       -  Then finds the tab content and adds the unselectedContent class */
    private void UnselectTab(Label tab)
    {
        tab.RemoveFromClassList(CurrentlySelectedTabClassName);
        tab.AddToClassList(NonSelectedTabClassName);
        VisualElement content = FindContent(tab);
        content.AddToClassList(UnselectedContentClassName);
    }

    // Method to generate the associated tab content name by for the given tab name
    private static string GenerateContentName(Label tab) =>
        tab.name.Replace(TabNameSuffix, ContentNameSuffix);

    // Method that takes a tab as a parameter and returns the associated content element
    private VisualElement FindContent(Label tab)
    {
        return root.Q(GenerateContentName(tab));
    }
}
