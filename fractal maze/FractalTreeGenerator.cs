using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalTreeGenerator : MonoBehaviour
{
    public int maxIterations = 6;
    public float angle = 30f;
    public float length = 1f;
    public GameObject branchPrefab;
    public MazePathwayGenerator mazePathwayGenerator;
    public int maxDepth = -1;
    public GameObject player;
    public int minDepth = 5;
    public int blockedBranchIncrement = 10; // doesn't work as of recent, changing
    private float timer = 0f;
    private float bottomThreshold = -10f;
    private float playerStartYThreshold = 0f;

    private string ax = "F"; // easier for later step
    private string currentString;
    private List<string> rules = new List<string>();
    private List<GameObject> branches = new List<GameObject>();

    void Start()
    {
        GenerateTree();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        timer += Time.deltaTime;
    }

    void AdjustTreeDepth()
    {
        if (maxDepth == -1)
        {
            float cameraHeight = Camera.main.orthographicSize * 2;
            maxDepth = Mathf.FloorToInt(cameraHeight / (2 * length));
        }
    }

    void GenerateTree()
    {
        ClearExistingBranches();
        rules.Add("F[+F]F[-F]F"); // rules
        rules.Add("F[-F][+F]F");
        rules.Add("F[+F][-F]");
        currentString = ax;
        AdjustTreeDepth();
        for (int i = 0; i < maxIterations; i++)
        {
            currentString = ApplyRules(currentString);
        }
        DrawTree();
        mazePathwayGenerator.CreatePathways(branches.ConvertAll(branch => branch.transform)); // ConvertAll, notice
        AdjustCamera();
        SetPlayerStartPosition();
        SetBottomThreshold();
    }

    void ClearExistingBranches()
    {
        foreach (GameObject branch in branches)
        {
            Destroy(branch);
        }
        branches.Clear();
    }

    string ApplyRules(string input)
    {
        string result = "";
        foreach (char c in input) // C++
        {
            if (c == 'F')
            {
                result += rules[Random.Range(0, rules.Count)];
            }
            else
            {
                result += c.ToString();
            }
        }
        return result;
    }

    void DrawTree() // similar to C++
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Vector3 currentPosition = new Vector3(0, -Camera.main.orthographicSize + length, 0);
        float currentAngle = 0;
        int currentDepth = 0;

        foreach (char c in currentString)
        {
            if (c == 'F')
            {
                if (currentDepth > maxDepth)
                {
                    continue;
                }

                Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;
                Vector3 newPosition = currentPosition + direction * length;
                GameObject branch = Instantiate(branchPrefab, (currentPosition + newPosition) / 2, Quaternion.identity);
                branch.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
                branch.tag = "Pathway"; // set tag if not working
                branches.Add(branch);

                currentPosition = newPosition;
                currentDepth++;
                //Debug.Log($"Branch added at position: {currentPosition} with tag: {branch.tag}");
            }
            else if (c == '+')
            {
                currentAngle += angle;
            }
            else if (c == '-')
            {
                currentAngle -= angle;
            }
            else if (c == '[') // push state, C++ --> C#
            {
                transformStack.Push(new TransformInfo { position = currentPosition, angle = currentAngle, depth = currentDepth }); // TransformInfo call, notice
            }
            else if (c == ']') // pop state
            {
                TransformInfo ti = transformStack.Pop();
                currentPosition = ti.position;
                currentAngle = ti.angle;
                currentDepth = ti.depth;
            }
        }

        //Debug.Log($"Total branches generated: {branches.Count}");
    }

    void AdjustCamera() // my least favorite
    {
        float highestY = float.MinValue;
        float lowestY = float.MaxValue;
        float leftmostX = float.MaxValue;
        float rightmostX = float.MinValue;

        foreach (Transform branch in branches.ConvertAll(branch => branch.transform)) // ConvertAll
        {
            if (branch.position.y > highestY)
            {
                highestY = branch.position.y;
            }
            if (branch.position.y < lowestY)
            {
                lowestY = branch.position.y;
            }
            if (branch.position.x < leftmostX)
            {
                leftmostX = branch.position.x;
            }
            if (branch.position.x > rightmostX)
            {
                rightmostX = branch.position.x;
            }
        }

        float treeHeight = highestY - lowestY;
        float treeWidth = rightmostX - leftmostX;
        float margin = 1.5f;

        Camera.main.orthographicSize = Mathf.Max(treeHeight / 2, treeWidth / (2 * Camera.main.aspect)) + margin;
        Camera.main.transform.position = new Vector3((rightmostX + leftmostX) / 2, (highestY + lowestY) / 2, -10);
    }

    void SetPlayerStartPosition()
    {
        // random start position for player
        List<GameObject> validBranches = branches.FindAll(branch => branch.transform.position.y > playerStartYThreshold);
        if (validBranches.Count > 0)
        {
            GameObject startBranch = validBranches[Random.Range(0, validBranches.Count)];
            player.transform.position = startBranch.transform.position;
            //Debug.Log($"Player starts at: {startBranch.transform.position}");
        }
        //else
        //{
        //    Debug.LogError("No valid branches available for starting position!");
        //}
    }

    void SetBottomThreshold()
    {
        float lowestY = float.MaxValue;
        foreach (Transform branch in branches.ConvertAll(branch => branch.transform))
        {
            if (branch.position.y < lowestY)
            {
                lowestY = branch.position.y;
            }
        }
        bottomThreshold = lowestY + 0.5f; // adjusting required here
    }

    public void RegenerateTree(bool adjustDepth)
    {
        ClearExistingBranches();

        if (adjustDepth)
        {
            if (timer <= 15f)
            {
                maxDepth += 2;
            }
            else if (timer <= 30f)
            {
                maxDepth += 1;
            }
            else
            {
                maxDepth = Mathf.Max(minDepth, maxDepth - 1);
            }

            mazePathwayGenerator.minBlockedBranches += blockedBranchIncrement; // increase blocked branches
            playerStartYThreshold += 1f; // increase the starting y threshold
        }

        timer = 0f;
        GenerateTree();
    }

    public void RestartLevel()
    {
        RandomlyReplacePathwaysAndBlockedBranches();
        GenerateTree();
    }

    private void RandomlyReplacePathwaysAndBlockedBranches() // love this name
    {
        List<GameObject> allBranches = new List<GameObject>(branches);

        foreach (GameObject branch in allBranches)
        {
            if (branch.CompareTag("Pathway"))
            {
                if (Random.value < 0.5f)
                {
                    // replace pathway with blocked branch
                    GameObject blockedBranch = Instantiate(mazePathwayGenerator.blockedBranchPrefab, branch.transform.position, branch.transform.rotation);
                    blockedBranch.tag = "BlockedBranch";
                    branches.Add(blockedBranch);
                    Destroy(branch);
                    branches.Remove(branch);
                }
            }
            else if (branch.CompareTag("BlockedBranch"))
            {
                if (Random.value < 0.5f)
                {
                    // replace blocked branch with pathway
                    GameObject pathway = Instantiate(mazePathwayGenerator.pathwayPrefab, branch.transform.position, branch.transform.rotation);
                    pathway.tag = "Pathway";
                    branches.Add(pathway);
                    Destroy(branch);
                    branches.Remove(branch);
                }
            }
        }
    }

    public float GetBottomThreshold()
    {
        return bottomThreshold;
    }

    struct TransformInfo
    {
        public Vector3 position;
        public float angle;
        public int depth;
    }
}
