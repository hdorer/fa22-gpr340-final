/* Pac-Man ghost AI behavior is based on this article:
 * https://gameinternals.com/understanding-pac-man-ghost-behavior
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GhostBrain : MonoBehaviour {
    private enum Behavior {
        CHASE,
        SCATTER,
        FRIGHTENED
    }
    
    private const float CHASE_TIMER_START = 20;
    private const float SCATTER_TIMER_START_PHASE_0 = 7;
    private const float SCATTER_TIMER_START_PHASE_4 = 5;
    
    private int phase = 0;
    private Behavior behavior = Behavior.SCATTER;
    private float phaseTimer = SCATTER_TIMER_START_PHASE_0;

    [SerializeField] private Square scatterCorner;
    protected Square ScatterCorner { get => scatterCorner; }
    [SerializeField] private Square scatterOpposite;
    private bool targetOpposite = false;

    private Square lastKnownCrossroads;
    protected Square LastKnownCrossroads { get => lastKnownCrossroads; }
    private Square playerSquare { get => FindObjectOfType<PacManInput>().GetComponent<CharacterMovement>().square; }

    private bool stuck = false;
    private bool stuckThisFrame = false;

    private CharacterMovement movement;
    protected CharacterMovement Movement { get => movement; }
    private AIPathfind pathfind;
    protected AIPathfind Pathfind { get => pathfind; }

    private void Awake() {
        movement = GetComponent<CharacterMovement>();
        pathfind = GetComponent<AIPathfind>();
    }

    private void Start() {
        setPathFromBehavior(false);
    }

    private void OnEnable() {
        foreach(Crossroads crossroads in FindObjectsOfType<Crossroads>()) {
            crossroads.crossroadsReachedEvent += onCrossroadsReached;
        }
    }

    private void Update() {
        phaseTimer -= Time.deltaTime;
        if(phaseTimer <= 0 && phase < 7) {
            advancePhase();
        }

        stuckThisFrame = false;
    }

    private void OnDisable() {
        foreach(Crossroads crossroads in FindObjectsOfType<Crossroads>()) {
            crossroads.crossroadsReachedEvent -= onCrossroadsReached;
        }
    }

    private void advancePhase() {
        phase++;

        if(phase % 2 == 0) {
            behavior = Behavior.SCATTER;
            phaseTimer = phase > 4 ? SCATTER_TIMER_START_PHASE_4 : SCATTER_TIMER_START_PHASE_0;
            targetOpposite = false;
            setPathFromBehavior(false);
        } else {
            behavior = Behavior.CHASE;
            phaseTimer = CHASE_TIMER_START;
            if(lastKnownCrossroads != null) {
                setPathFromBehavior(false);
            } else {
                behavior = Behavior.SCATTER;
            }
        }
    }

    private void onCrossroadsReached(CrossroadsReachedEventArgs e) {
        lastKnownCrossroads = e.square;
        setPathFromBehavior(false);
    }

    private void setPathFromBehavior(bool chasePlayer) {
        switch(behavior) {
            case Behavior.SCATTER:
                pathfind.setPath(targetOpposite ? scatterOpposite : scatterCorner);
                break;
            case Behavior.CHASE:
                pathfind.setPath(chasePlayer ? playerSquare : getChaseTarget());
                break;
        }
    }

    protected abstract Square getChaseTarget();

    public void chooseRandomDirection() {
        stuck = true;
        stuckThisFrame = true;

        List<Vector2Int> directions = new List<Vector2Int>() { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        if(directions.Contains(movement.Direction)) {
            directions.Remove(movement.Direction);
        }
        
        Vector2Int nextDirection = movement.Direction;
        bool validDirection = movement.canMoveIntoSquare(nextDirection);
        while(!validDirection) {
            int roll = UnityEngine.Random.Range(0, directions.Count - 1);

            nextDirection = directions[roll];
            validDirection = movement.canMoveIntoSquare(nextDirection);
            directions.Remove(directions[roll]);
        }

        movement.Direction = nextDirection;
    }

    public void remakePath() {
        if(stuck && !stuckThisFrame) {
            setPathFromBehavior(false);
            stuck = false;
        }
    }

    public void updatePath() {
        if(behavior == Behavior.SCATTER) {
            targetOpposite = !targetOpposite;
        }
        setPathFromBehavior(true);
    }
}
