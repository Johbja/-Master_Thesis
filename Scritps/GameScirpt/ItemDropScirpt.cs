using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem {
    public GameObject item;
    public int dropChans;
}

public class ItemDropScirpt : MonoBehaviour {

    [SerializeField] private DropItem[] itemsToDrop;
    [SerializeField] private int dropAmount;
    [SerializeField] private float dropForce;

    public void DropItem() {
        for(int i = 0; i < dropAmount; i++) {
            DropItem item = GetDropItem(Random.Range(0, GetDropChanseSum()));

            if(item.item == null)
                return;

            GameObject instance = Instantiate(item.item, transform.position, Quaternion.identity);

            float randAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
            Vector2 randDir = new Vector2(Mathf.Cos(randAngle), Mathf.Sin(randAngle));
            instance.GetComponent<Rigidbody2D>().AddForce(randDir * dropForce, ForceMode2D.Impulse);

            GetComponent<AudioSource>().Play();
        }
    }

    public List<GameObject> DropInventory(GameObject[] items) {
        List<GameObject> droppedItems = new List<GameObject>();
        for(int i = 0; i < items.Length; i++) {
            GameObject instance = Instantiate(items[i], transform.position, Quaternion.identity);
            instance.GetComponent<PickupHandler>().ChangeWeapon(items[i].GetComponent<PickupHandler>().GetWeapon());
            float randAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
            Vector2 randDir = new Vector2(Mathf.Cos(randAngle), Mathf.Sin(randAngle));
            instance.GetComponent<Rigidbody2D>().AddForce(randDir * dropForce, ForceMode2D.Impulse);
            droppedItems.Add(instance);
        }

        return droppedItems;
    }

    private int GetDropChanseSum() {
        int sum = 0;

        for(int i = 0; i < itemsToDrop.Length; i++)
            sum += itemsToDrop[i].dropChans;

        return sum;
    }

    private DropItem GetDropItem(int cutoff) {
        int sum = 0;
        for(int i = 0; i < itemsToDrop.Length; i++) {
            sum += itemsToDrop[i].dropChans;
            if(sum >= cutoff)
                return itemsToDrop[i];
        }
        return itemsToDrop[0];
    }

}
