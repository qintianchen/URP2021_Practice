using UnityEngine;
using Random = UnityEngine.Random;

public class ConstInHLSL : MonoBehaviour
{
    public GameObject prefab;
    public int        count;
    private void OnEnable()
    {
        for (var i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            var x  = Random.Range(-5f, 5);
            var y  = Random.Range(-5f, 5);
            var z  = Random.Range(-5f, 5);
            go.transform.localPosition = new Vector3(x, y, z);
        }
    }
}
