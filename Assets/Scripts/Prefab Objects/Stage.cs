using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Stage {
    private string m_id;

    private GameObject m_object;
    private GameObject m_groundSnapping;
    private List<Transform> m_bridgeGrounds = new List<Transform>();
    private const float m_gravity = 9.81f;

    public float leftBound { get; private set; }
    public float rightBound { get; private set; }
    
    public float heroSpawn { get; private set; }
    public float allySpawn { get; private set; }
    public float zombieSpawn { get; private set; }

    public Stage(string stageId) {
        m_id = stageId;
    }

    public async Task Init() {
        // Load stage prefab.
        var objectHandle = Addressables.LoadAssetAsync<GameObject>($"Stages/{m_id}");
        m_object = await objectHandle.Task;
        if (m_object == null) {
            Debug.LogError($"Could not find or load stage of m_id `{m_id}`.");
            return;
        }
        m_object = Object.Instantiate(m_object);
        m_object.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Sort ground snapping for bridge.
        Transform buffer = m_object.transform.Find("Ground Snapping");
        if (buffer == null) {
            Debug.LogError($"Could not find essential child object \"Ground Snapping\" in stage `{m_id}`.");
            return;
        }
        m_groundSnapping = buffer.gameObject;
        for (int i = 0; true; i++) {
            buffer = m_groundSnapping.transform.Find("Ground " + (i + 1));
            if (buffer == null) break;
            m_bridgeGrounds.Add(buffer);
        }
        m_groundSnapping.SetActive(false);

        // Get left and right x bounds.
        Transform bounds = m_object.transform.Find("Bounds");
        leftBound = bounds.Find("Left").position.x;
        rightBound = bounds.Find("Right").position.x;

        Transform spawnPoints = m_object.transform.Find("Spawn Points");
        heroSpawn = spawnPoints.Find("Hero Spawn").position.x;
        allySpawn = spawnPoints.Find("Ally Spawn").position.x;
        zombieSpawn = spawnPoints.Find("Zombie Spawn").position.x;
    }

    public void ApplyGravity(GameplayEntity entity) {
        Transform gameObjectTransform = entity.transform;
        float groundY = 0;
        Vector3 gameObjectPos = gameObjectTransform.position;
        for (int i = 0; i < m_bridgeGrounds.Count - 1; i++) {
            float leftX = m_bridgeGrounds[i].position.x;
            float rightX = m_bridgeGrounds[i + 1].position.x;

            if (gameObjectPos.x >= leftX && gameObjectPos.x <= rightX) {
                Vector3 leftPos = m_bridgeGrounds[i].position;
                Vector3 rightPos = m_bridgeGrounds[i + 1].position;
                leftPos.y += m_bridgeGrounds[i].localScale.y / 2f;
                rightPos.y += m_bridgeGrounds[i + 1].localScale.y / 2f;
                float t = Mathf.InverseLerp(leftX, rightX, gameObjectPos.x);
                float interpY = Mathf.Lerp(leftPos.y, rightPos.y, t);
                groundY = interpY;
                break;
            }
        }

        entity.yVelocity -= m_gravity * Time.deltaTime;
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