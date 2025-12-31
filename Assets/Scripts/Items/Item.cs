using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
fileName = "NewItemData",
menuName = "New Item Data"
)]

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemImg;

    public int quantityLimit;

    public bool selected;

    // List used in the inspector
    public List<TreeEntry> craftingTreeLists = new List<TreeEntry>();

    public Dictionary<string, CraftingNode> craftingNodes = new Dictionary<string, CraftingNode>();

    public void InitializeDictionary()
    {
        Debug.Log(itemName);
        foreach (TreeEntry craftingTree in craftingTreeLists)
        {
            craftingNodes[craftingTree.treeName] = craftingTree.node;
            printTrees();
        }
    }


    public GameObject worldPrefab;

    public ItemAction action;

    public void printTrees()
    {
        Debug.Log(itemName + "CURRENT ITEM");
        if (craftingNodes[craftingTreeLists[0].treeName].parent) Debug.Log(craftingNodes[craftingTreeLists[0].treeName].parent.itemName + "PARENT");
        CraftingNode[] children = craftingNodes[craftingTreeLists[0].treeName].getChildren();
        int x = 0;
        foreach (CraftingNode node in children)
        {
            if (node.item != null) Debug.Log(x.ToString() + " Child: " + node.item.itemName);
            x++;
        }
    }


    public void Use(GameObject gameManager)
    {
        Debug.Log("Trying to use");
        if (action!=null)
        {
            action.Execute(this, gameManager);
        }
        else
        {
            Debug.Log("Did not execute item action");
        }
    }

    public bool hasTree(string name)
    {
        return (craftingNodes.ContainsKey(name));
    }
}

[System.Serializable]
public class TreeEntry
{
    public string treeName;       // Key
    public CraftingNode node;   // Value
}

[System.Serializable]
public class CraftingNode
{

    public Item item;

    public string treeName;

    // The parent in this tree (null if root)
    public Item parent = null;

    // The children in this tree
    public CraftingNode[] children = new CraftingNode[3];

    public CraftingNode[] getChildren()
    {
        return children;
    }

    public List<CraftingNode> getAllChildrenOfParent()
    {
        CraftingNode parentNode = this.parent.craftingNodes[treeName];
        return new List<CraftingNode>(parentNode.children);
    }
}
