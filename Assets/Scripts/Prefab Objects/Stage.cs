using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Stage {
    private WaveData.Stage stage;

    private GameObject gameObject;
    private GameObject groundSnapping;
    private List<Transform> bridgeGrounds = new List<Transform>();
    private const float gravity = 9.81f;

    public float leftBound { get; private set; }
    public float rightBound { get; private set; }
    
    public float heroSpawn { get; private set; }
    public float allySpawn { get; private set; }
    public float zombieSpawn { get; private set; }

    public Stage(WaveData.Stage _stage) {
        stage = _stage;
    }

    public async Task Init() {
        // Load stage prefab.
        var objectHandle = Addressables.LoadAssetAsync<GameObject>($"Stages/{WaveData.StageToString(stage)}");
        gameObject = await objectHandle.Task;
        if (gameObject == null) {
            Debug.LogError($"Could not find or load stage of ID `{WaveData.StageToString(stage)}`.");
            return;
        }
        gameObject = Object.Instantiate(gameObject);
        gameObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Sort ground snapping for bridge.
        Transform buffer = gameObject.transform.Find("Ground Snapping");
        if (buffer == null) {
            Debug.LogError($"Could not find essential child object \"Ground Snapping\" in stage `{WaveData.StageToString(stage)}`.");
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
        Transform bounds = gameObject.transform.Find("Bounds");
        leftBound = bounds.Find("Left").position.x;
        rightBound = bounds.Find("Right").position.x;

        Transform spawnPoints = gameObject.transform.Find("Spawn Points");
        heroSpawn = spawnPoints.Find("Hero Spawn").position.x;
        allySpawn = spawnPoints.Find("Ally Spawn").position.x;
        zombieSpawn = spawnPoints.Find("Zombie Spawn").position.x;
    }

    public void ApplyGravity(GameplayEntity entity) {
        Transform gameObjectTransform = entity.transform;
        float groundY = 0;
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
                groundY = interpY;
                break;
            }
        }

        entity.yVelocity -= gravity * Time.deltaTime;
        gameObjectTransform.position += new Vector3(0, entity.yVelocity, 0) * Time.deltaTime;
        if (gameObjectTransform.position.y < groundY) {
            gameObjectTransform.position = new Vector3(gameObjectTransform.position.x, groundY, gameObjectTransform.position.z);
            entity.yVelocity = 0;
            entity.isGettingKnockedBack = false;
        } else {
            entity.isGettingKnockedBack = true;
        }
    }
};