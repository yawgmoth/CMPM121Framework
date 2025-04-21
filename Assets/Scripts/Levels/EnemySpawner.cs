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
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    private int currentWave = 1;

    public Dictionary<string, Enemy> enemy_types;
    public Dictionary<string, Level> levels;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");

        //Json Deserialization
        enemy_types = new Dictionary<string, Enemy>();
        var enemytext = Resources.Load<TextAsset>("enemies");
        JToken jo = JToken.Parse(enemytext.text);
        foreach (var enemy in jo)
        {
            Enemy en = enemy.ToObject<Enemy>();
            enemy_types[en.name] = en;
        }
        levels = new Dictionary<string, Level>();
        var leveltext = Resources.Load<TextAsset>("levels");
        JToken jol = JToken.Parse(leveltext.text);
        foreach (var level in jol)
        {
            Level lvl = level.ToObject<Level>();
            levels[lvl.name] = lvl;
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

    public int RPN_to_int(string rpn)
    {

        Stack<int> stack = new Stack<int>();
        string[] tokens = rpn.Split(' ');

        foreach (string token in tokens)
        {
            if (token == "wave")
            {
                stack.Push(currentWave);
            }
            else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
            {
                

                int b = stack.Pop();
                int a = stack.Pop();
                if (token == "+"){
                    stack.Push(a + b);
                } else if (token == "-"){
                    stack.Push(a - b);
                } else if (token == "*"){
                    stack.Push(a * b);
                } else if (token == "/"){
                    stack.Push(a / b);
                } else if (token == "%"){
                    stack.Push(a % b);
                }
            }
            else
            {
                if (int.TryParse(token, out int value))
                {
                    stack.Push(value);
                }
                else
                {
                    return 0;
                }
            }
        }

        return stack.Count > 0 ? stack.Pop() : 0;
    }



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

        string level_name = "Easy";
        Level level = levels[level_name];
        foreach (var wave in level.spawns)
        {
            StartCoroutine(ManageWave(wave));
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        currentWave += 1;
        if (currentWave > level.waves){
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        }
        else {
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
        }
        
        
    }

    IEnumerator ManageWave(Spawn spawn)
    {
        // int spawned = 0;
        // while (spawned < 10)
        // {
        //     int num_to_spawn = 5;
        //     for (int i = 0; i < num_to_spawn; i++)
        //     {
        //         yield return SpawnEnemy(spawn.enemy);
        //         spawned++;
        //     }
        //     yield return new WaitForSeconds(spawn.delay);
        // }

        int total_to_spawn = RPN_to_int(spawn.count); // e.g., "5 wave +" should become int
        int spawned = 0;
        Debug.Log(total_to_spawn);

        float delay = spawn.delay > 0 ? spawn.delay : 1f;

        while (spawned < total_to_spawn)
        {
            yield return SpawnEnemy(spawn.enemy);
            spawned++;
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator SpawnEnemy(string enemy_name)
    {

        Enemy enemy_stats = enemy_types[enemy_name];
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemy_stats.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enemy_stats.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemy_stats.speed;
        en.damage = enemy_stats.damage;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }


}
