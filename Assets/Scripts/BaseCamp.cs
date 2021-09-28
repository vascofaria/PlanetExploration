using UnityEngine;
using System.Collections.Generic;
public class BaseCamp : MonoBehaviour {

    private List<Resource> collectedResources = new List<Resource>();

    public Vector3 position => transform.position;

    private void Awake() {
        World.baseCamp = this;
    }

    public void StoreResource(Resource resource) { 
        
        if (ContainsResource(resource))
            return;

        collectedResources.Add(resource);

        switch(resource.resourceType) {
            case ResourceType.SMALL : {
                UIManager.uIManager.CountSmallResources(1);
                break;
            }
            case ResourceType.MEDIUM : {
                UIManager.uIManager.CountMediumResources(1);
                break;
            }
            case ResourceType.LARGE : {
                UIManager.uIManager.CountLargeResources(1);
                break;
            }
        }
    }


    private bool ContainsResource(Resource resource) {
        foreach (Resource r in collectedResources)
        {
            if (r.resourceType == resource.resourceType && r.resource == resource.resource)
                return true;
        }
        return false;
    } 
    


}