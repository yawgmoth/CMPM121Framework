using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public SpawnPoint[] SpawnPoints; 
    public GameObject enemy;
    public Dictionary<string, EnemyType> enemy_list;
    public Dictionary<string, LevelData> level_list;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
        
        //setup enemy types
        enemy_list = new Dictionary<string, EnemyType>();
        var enemytext = Resources.Load<TextAsset>("enemies");

        JToken jo = JToken.Parse(enemytext.text);
        foreach(var e in jo) {
            EnemyType en = e.ToObject<EnemyType>();
            enemy_list[en.name] = en;
        }


        //setup level types
        level_list = new Dictionary<string, LevelData>();
        var leveltext = Resources.Load<TextAsset>("levels");

        jo = JToken.Parse(leveltext.text);
        foreach(var l in jo) {
            LevelData lv = l.ToObject<LevelData>();
            level_list[lv.name] = lv;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }

    //TODO have the spawnwave function below read from the JSON file for time delay?
    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        for (int i = 0; i < 10; ++i)
        {
            yield return Spawn("skeleton");
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }
    //TODO modify both functions below to read from the JSON file
    /*IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }*/

    IEnumerator Spawn(string enemy_name)
    {
        //pick enemey type
        EnemyType enemy_type = enemy_list[enemy_name];
        //pick spawnpoint
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemy_type.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enemy_type.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemy_type.speed;
        en.damage = enemy_type.damage;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}

public class EnemyType {
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

public class LevelData {
    public string name;
    public int waves;
    public string[] spawns;
    public SpawnData[] spawn_data;
    //TODO: parse jsson object of spawns to turn into wavee spawning info
    public bool process_spawn_data() {
        return true;
    }
}

public class SpawnData {
    public string enemy;
    public int count;
    public string hp;
    public int delay;
    public int[] sequence;
    public string location;
}