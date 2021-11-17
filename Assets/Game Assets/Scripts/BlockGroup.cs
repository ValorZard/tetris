using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockGroup : MonoBehaviour {

    // time of the last fall, used to auto fall after 
    // time parametrized by `level`
    private float lastFall;

    // last key pressed time, to handle long press behavior
    private float lastKeyDown;
    private float timeKeyPressed;

    public void AlignCenter() {
        transform.position += transform.position - Utils.Center(gameObject);
    }


    bool isValidPlayfieldPos() {
        foreach (Transform child in transform) {
            Vector2 v = Playfield.roundVector2(child.position);

            // not inside Border?
            if(!Playfield.insideBorder(v)) {
                return false;
            }

            // Block in grid cell (and not par of same group)?
            if (Playfield.grid[(int)(v.x), (int)(v.y)] != null &&
                Playfield.grid[(int)(v.x), (int)(v.y)].parent != transform) {
                return false;
            }
        }

        return true;
    }

    void removeSelfFromPlayfield()
    {
        // remove all children from grid
        for (int y = 0; y < Playfield.height; ++y)
        {
            for (int x = 0; x < Playfield.width; ++x)
            {
                if (Playfield.grid[x, y] != null &&
                    Playfield.grid[x, y].parent == transform)
                {
                    Playfield.grid[x, y] = null;
                }
            }
        }
    }

    void insertOnPlayfield()
    {
        // add new children to grid
        foreach (Transform child in transform)
        {
            Vector2 v = Playfield.roundVector2(child.position);
            Playfield.grid[(int)v.x, (int)v.y] = child;
        }
    }

    // update the playfield
    public void updatePlayfield() {
        // Remove old children from grid
        removeSelfFromPlayfield();

        insertOnPlayfield();
    }

    void gameOver() {
        Debug.Log("GAME OVER!");
        while (!isValidPlayfieldPos()) {
            //Debug.LogFormat("Updating last group...: {0}", transform.position);
            transform.position  += new Vector3(0, 1, 0);
        } 
        updatePlayfield(); // to not overleap invalid groups
        enabled = false; // disable script when dies
        UIController.gameOver(); // active Game Over panel
        Highscore.Set(ScoreManager.score); // set highscore
        //Music.stopMusic(); // stop Music
    }

    // Use this for initialization
    void Start () {
        lastFall = Time.time;
        lastKeyDown = Time.time;
        timeKeyPressed = Time.time;
        if (isValidPlayfieldPos()) {
            insertOnPlayfield();
        } else { 
            Debug.Log("KILLED ON START");
            gameOver();
        }

    }

    void tryChangePos(Vector3 v) {
        // modify position 
        // FIXME: maybe this is idiot? I can create a copy before and only assign to the transform if is valid
        transform.position += v;

        // See if valid
        if (isValidPlayfieldPos()) {
            updatePlayfield();
        } else {
            transform.position -= v;
        }
    }

    void fallGroup() {
        // modify
        transform.position += new Vector3(0, -1, 0);

        if (isValidPlayfieldPos()){
            // It's valid. Update Playfield... again
            updatePlayfield();
        } else {
            // it's not valid. revert
            transform.position += new Vector3(0, 1, 0);

            // Clear filled horizontal lines
            Playfield.deleteFullRows();


            FindObjectOfType<Spawner>().spawnNext();


            // Disable script
            enabled = false;
        }

        lastFall = Time.time;

    }

    // getKey if is pressed now on longer pressed by 0.5 seconds | if that true apply the key each 0.05f while is pressed
    bool getKey(KeyCode key) {
        bool keyDown = Input.GetKeyDown(key);
        bool pressed = Input.GetKey(key) && Time.time - lastKeyDown > 0.5f && Time.time - timeKeyPressed > 0.05f;

        if (keyDown) {
            lastKeyDown = Time.time;
        }
        if (pressed) {
            timeKeyPressed = Time.time;
        }
 
        return keyDown || pressed;
    }


    // Update is called once per frame
    void Update () {
        if (UIController.isPaused) {
            return; // don't do nothing
        }
        if (getKey(KeyCode.LeftArrow))
        {
            tryChangePos(new Vector3(-1, 0, 0));
        }
        else if (getKey(KeyCode.RightArrow))
        {  // Move right
            tryChangePos(new Vector3(1, 0, 0));
        }

        if (getKey(KeyCode.UpArrow) && gameObject.tag != "Cube") { 
            // Rotate Right
            transform.Rotate(0, 0, -90);

            // see if valid
            if (isValidPlayfieldPos()) {
                // it's valid. Update grid
                updatePlayfield();
            } else {
                // it's not valid. revert
                transform.Rotate(0, 0, 90);
            }
        }
        else if (getKey(KeyCode.Z) && gameObject.tag != "Cube")
        { // Rotate Left
            transform.Rotate(0, 0, 90);

            // see if valid
            if (isValidPlayfieldPos())
            {
                // it's valid. Update grid
                updatePlayfield();
            }
            else
            {
                // it's not valid. revert
                transform.Rotate(0, 0, -90);
            }
        
        }
        if (getKey(KeyCode.DownArrow) || (Time.time - lastFall) >= (float)1 / Mathf.Sqrt(LevelManager.level)) {
            fallGroup();
        } else if (Input.GetKeyDown(KeyCode.Space)) {
            while (enabled) { // fall until the bottom 
                fallGroup();
            }
        }

        // replace current block with stored block
        if (Input.GetKeyDown(KeyCode.C))
        {
            StoreBox storeBox = FindObjectOfType<StoreBox>();
            // nothings stored there
            if (storeBox.storedGroupObject == null)
            {
                storeBox.storeBlockGroup(this.gameObject);
                FindObjectOfType<Spawner>().spawnNext(); // spawn new block
                // Disable script
                enabled = false;
                removeSelfFromPlayfield();
            }
            else
            {
                // remove self from playfield
                removeSelfFromPlayfield();
                // add stored block group to the playfield
                GameObject blockGroup = storeBox.getStoredBlockGroup();
                blockGroup.GetComponent<Transform>().position = FindObjectOfType<Spawner>().GetComponent<Transform>().position;
                blockGroup.GetComponent<BlockGroup>().enabled = true;
                blockGroup.GetComponent<BlockGroup>().updatePlayfield();
                // disable script, and store old block group into store box
                storeBox.storeBlockGroup(this.gameObject);
                enabled = false;
            }
        }

    }
}
