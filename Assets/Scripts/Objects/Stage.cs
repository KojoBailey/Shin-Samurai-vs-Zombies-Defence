using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class Stage {
    private string id;

    private GameObject obj;
    private GameObject groundSnapping;
    private List<Transform> bridgeGrounds = new List<Transform>();

    public float LeftBound { get; private set; }
    public float RightBound { get; private set; }
    
    public float HeroSpawn { get; private set; }
    public float AllySpawn { get; private set; }
    public float ZombieSpawn { get; private set; }

    public Stage(string stageId) {
        id = stageId;
    }

    public async Task Init() {
        // Load stage prefab.
        var objectHandle = Addressables.LoadAssetAsync<GameObject>($"Stages/{id}");
        obj = await objectHandle.Task;
        if (obj == null) {
            Debug.LogError($"Could not find or load stage of id `{id}`.");
            return;
        }
        obj = Object.Instantiate(obj);
        obj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Sort ground snapping for bridge.
        Transform buffer = obj.transform.Find("Ground Snapping");
        if (buffer == null) {
            Debug.LogError($"Could not find essential child object \"Ground Snapping\" in stage `{id}`.");
            return;
        }
        groundSnapping = buffer.gameObject;
        for (int i = 0; true; i++) {
            buffer = groundSnapping.transform.Find("Ground " + (i + 1));
            if (buffer == null) break;
            bridgeGrounds.Add(buffer);
        }
        groundSnapping.SetActive(false);

        // Get left and right x bounds.
        Transform bounds = obj.transform.Find("Bounds");
        LeftBound = bounds.Find("Left").position.x;
        RightBound = bounds.Find("Right").position.x;

        Transform spawnPoints = obj.transform.Find("Spawn Points");
        HeroSpawn = spawnPoints.Find("Hero Spawn").position.x;
        AllySpawn = spawnPoints.Find("Ally Spawn").position.x;
        ZombieSpawn = spawnPoints.Find("Zombie Spawn").position.x;
    }

    public void SnapToGround(Transform gameObjectTransform) {
        Vector3 gameObjectPos = gameObjectTransform.position;
        for (int i = 0; i < bridgeGrounds.Count - 1; i++) {
            float leftX = bridgeGrounds[i].position.x;
            float rightX = bridgeGrounds[i + 1].position.x;

            if (gameObjectPos.x >= leftX && gameObjectPos.x <= rightX) {
                Vector3 leftPos = bridgeGrounds[i].position;
                Vector3 rightPos = bridgeGrounds[i + 1].position;
                leftPos.y += bridgeGrounds[i].localScale.y / 2f;
                rightPos.y += bridgeGrounds[i + 1].localScale.y / 2f;
                float t = Mathf.InverseLerp(leftX, rightX, gameObjectPos.x);
                float interpY = Mathf.Lerp(leftPos.y, rightPos.y, t);
                gameObjectTransform.position = new Vector3(gameObjectPos.x, interpY, gameObjectPos.z);
                break;
            }
        }
    }
};