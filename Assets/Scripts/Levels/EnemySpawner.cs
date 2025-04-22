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
    public string difficulty_level;
    public int wave;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Easy");

        selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 90);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Medium");

        selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 50);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Endless");
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
        difficulty_level = levelname;
        wave = 1;
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
        wave++;
    }

    public static int evaluateRPN(string expression)
    {
        Stack<int> stack = new Stack<int>();

        if (string.IsNullOrWhiteSpace(expression))
        {
            Debug.LogWarning("Expression is empty or null.");
            return int.MinValue;
        }

        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else
            {
                if (stack.Count < 2)
                {
                    Debug.LogWarning("Invalid RPN expression: Not enough operands.");
                    return int.MinValue;
                }

                int b = stack.Pop();
                int a = stack.Pop();

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/":
                        if (b == 0)
                        {
                            Debug.LogWarning("Division by zero.");
                            return int.MinValue;
                        }
                        stack.Push(a / b);
                        break;
                    case "%":
                        if (b == 0)
                        {
                            Debug.LogWarning("Modulo by zero.");
                            return int.MinValue;
                        }
                        stack.Push(a % b);
                        break;
                    default:
                        Debug.LogWarning($"Unknown operator: {token}");
                        return int.MinValue;
                }
            }
        }

        if (stack.Count == 1)
            return stack.Pop();

        Debug.LogWarning("Invalid RPN expression: leftover operands.");
        return int.MinValue;
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
        

        var stage = GameManager.Instance.level_types[difficulty_level];

        foreach(var spawn in stage.spawns){
            
            var count = evaluateRPN(spawn.count.Replace("wave", wave.ToString()));
            Debug.Log("Spawning: " + count);
            for(int i = 0; i < count; i++){
                yield return SpawnEnemy(spawn);
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnEnemy(Spawn spawn)
    {
        int delay;
        // int delay = int.TryParse(spawn.delay, delay);
        int hp = evaluateRPN(spawn.hp.Replace("wave", wave.ToString()).Replace("base", GameManager.Instance.enemy_types[spawn.enemy].hp.ToString()));

        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        if(spawn.location == "random"){
            spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else if (spawn.location == "random red"){
            spawn_point = SpawnPoints[Random.Range(4, SpawnPoints.Length)];
        }
        else if (spawn.location == "random bone"){
            spawn_point = SpawnPoints[3];
        }

        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(GameManager.Instance.enemy_types[spawn.enemy].sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();

        en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        
        GameManager.Instance.AddEnemy(new_enemy);
        
        if(int.TryParse(spawn.delay, out delay)){   
            yield return new WaitForSeconds(delay);
        }
        else {
            yield return new WaitForSeconds(2);
        }


    }
}
