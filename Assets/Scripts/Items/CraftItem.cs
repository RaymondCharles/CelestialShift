using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftItem : MonoBehaviour
{
    private SlotItem slotItem1;
    private SlotItem slotItem2;
    private SlotItem slotItem3;
    private SlotItem newItem;

    Item item1;
    Item item2;
    Item item3;

    public CraftingUI craftingUI;
    private bool superCrafting = false;
    
    public void craftItem()
    {
        slotItem1 = craftingUI.craftingItems[0];
        slotItem2 = craftingUI.craftingItems[1];

        Item item1 = slotItem1.itemDetails;
        Item item2 = slotItem2.itemDetails;

        if (craftingUI.isSuperCrafting())
        {
            Debug.Log("SuperCrafting");
            slotItem3 = craftingUI.craftingItems[2];
            Item item3 = slotItem3.itemDetails;
            superCrafting = true;
        }
        else
        {
            Debug.Log("NormalCrafting");
        }

        List<string> possibleTrees = new List<string>();
        foreach (TreeEntry tree in item1.craftingTreeLists)
        {
            string name = tree.treeName;
            Debug.Log(name);
            foreach (string key in item2.craftingNodes.Keys)
            {
                Debug.Log(key + "THIS IS A KEY");
            }

            if (item2.craftingNodes.ContainsKey(name) && (!superCrafting || item3.craftingNodes.ContainsKey(name)))
            {
                Debug.Log(name);
                CraftingNode item1Node = tree.node;
                CraftingNode item2Node = item2.craftingNodes[name];
                List<CraftingNode> nodes = new List<CraftingNode>{item1Node, item2Node};

                if (superCrafting) 
                {
                    CraftingNode item3Node = item3.craftingNodes[name];
                    nodes.Add(item3Node);
                    if (item1Node.parent != item3Node.parent) continue;
                }
        

                if (item1Node.parent != item2Node.parent)
                {
                    continue;
                }

                List<CraftingNode> allRequiredChildren = item1Node.getAllChildrenOfParent();

                bool failCraft = false;
                foreach (CraftingNode node in nodes)
                {
                    if (!allRequiredChildren.Remove(node)) failCraft = true;
                }
                if (failCraft) continue;
                
                if (allRequiredChildren.Count == 0)
                {
                    newItem = new SlotItem(item1Node.parent, 1);
                }
            }
        }
        Debug.Log("No crafting tree found");
        return;
    }

}
