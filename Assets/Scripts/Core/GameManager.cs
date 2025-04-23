using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class GameManager 
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;

    public int countdown;
    private static GameManager theInstance;
    public static GameManager Instance {  get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    public GameObject player;
    
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;

    public float timeStart = 0;
    public float timeEnd = 0;
    public float timeSpent = 0;
    public int damageDealt = 0;
    public int damageReceived = 0;
    public Dictionary<string, Enemy> enemy_types;
    public Dictionary<string, Level> level_types;
    private List<GameObject> enemies;
    public int enemy_count { get { return enemies.Count; } }

    public void ParseEnemyJSON(){
        enemy_types = new Dictionary<string, Enemy>();
        var enemytext = Resources.Load<TextAsset>("enemies");

        // var convert = JsonConvert.DeserializeObject<List<Enemy>>(enemytext.text);
        
        JToken jo = JToken.Parse(enemytext.text);
        foreach (var enemy in jo)
        {
            Enemy en = enemy.ToObject<Enemy>();
            enemy_types[en.name] = en;
        }
    }
    public void ParseLevelJSON(){
        level_types = new Dictionary<string, Level>();
        var leveltext = Resources.Load<TextAsset>("levels");
        JToken jo = JToken.Parse(leveltext.text);
        foreach (var level in jo)
        {
            Level le = level.ToObject<Level>();
            level_types[le.name] = le;
        }
    }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a,b) => (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        enemies = new List<GameObject>();
        ParseEnemyJSON();
        ParseLevelJSON();
    }
}
