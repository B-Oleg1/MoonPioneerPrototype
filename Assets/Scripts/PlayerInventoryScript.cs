using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryScript : MonoBehaviour
{
    [SerializeField]
    private Transform _inventory;

    private List<GameObject> _resourcesPool;

    public int[] allResources;

    private string[] _nameResources = { "FirstResources", "SecondResources", "ThirdResources" };

    private void Start()
    {
        _resourcesPool = new List<GameObject>();

        allResources = new int[10];
    }

    public void AddResource(int idResource)
    {
        var resource = _resourcesPool.FirstOrDefault(item => item.tag.ToLower() == _nameResources[idResource].ToLower() && !item.activeSelf);
        if (resource != null)
        {
            resource.SetActive(true);
        }
        else
        {
            resource = Instantiate(Resources.Load<GameObject>(_nameResources[idResource]), _inventory);
            _resourcesPool.Add(resource);
        }

        UpdateInventory();
    }

    public void RemoveResource(int idResource)
    {
        _resourcesPool.FirstOrDefault(item => item.tag == _nameResources[idResource] && item.activeSelf)?.SetActive(false);

        UpdateInventory();
    }

    public void UpdateInventory()
    {
        Array.Sort(allResources);
        Array.Reverse(allResources);

        int i = 0;
        while (i < allResources.Length && allResources[i] != 0)
        {
            int a = 0;
            while (a < _inventory.childCount)
            {
                GameObject item = _inventory.GetChild(a).gameObject;
                if (item.tag.ToLower() == _nameResources[allResources[i] - 1].ToLower() && item.activeSelf && item.transform.parent == _inventory)
                {
                    item.transform.localPosition = new Vector3(0, i * 0.2f, 0);
                    i++;

                    if (i == allResources.Length || allResources[i] == 0)
                    {
                        break;
                    }
                }

                a++;
            }
        }
    }
}