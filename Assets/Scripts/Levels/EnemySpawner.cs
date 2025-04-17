using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

//test

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
        GameObject selector_eas = Instantiate(button, level_selector.transform);
        selector_eas.transform.localPosition = new Vector3(0, 110);
        selector_eas.GetComponent<MenuSelectorController>().spawner = this;
        selector_eas.GetComponent<MenuSelectorController>().SetLevel("Easy");

        GameObject selector_med = Instantiate(button, level_selector.transform);
        selector_med.transform.localPosition = new Vector3(0, 60);
        selector_med.GetComponent<MenuSelectorController>().spawner = this;
        selector_med.GetComponent<MenuSelectorController>().SetLevel("Medium");

        GameObject selector_har = Instantiate(button, level_selector.transform);
        selector_har.transform.localPosition = new Vector3(0, 0);
        selector_har.GetComponent<MenuSelectorController>().spawner = this;
        selector_har.GetComponent<MenuSelectorController>().SetLevel("Hard");
        
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
        Debug.Log(levelname);
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