using System.Collections.Generic;
using UnityEngine;

public class MazePathwayGenerator : MonoBehaviour
{
    public GameObject pathwayPrefab;
    public GameObject blockedBranchPrefab;
    public float additionalPathProbability = 0.2f;
    public int minBlockedBranches = 10; // stopped working past a certain point :-)
    public int extraBlockedBranches = 20; // will this work?

    private List<GameObject> pathways = new List<GameObject>();
    private List<GameObject> blockedBranches = new List<GameObject>();

    public void CreatePathways(List<Transform> branches)
    {
        HashSet<Transform> mainPath = new HashSet<Transform>();
        List<Transform> branchStack = new List<Transform>();

        Transform highestBranch = GetHighestBranch(branches);
        Transform lowestBranch = GetLowestBranch(branches);

        //Debug.Log($"Highest Branch: {highestBranch.position}");
        //Debug.Log($"Lowest Branch: {lowestBranch.position}");

        branchStack.Add(highestBranch);
        mainPath.Add(highestBranch);

        CreatePathDFS(highestBranch, lowestBranch, mainPath, branches);

        mainPath.Add(lowestBranch);

        foreach (Transform branch in mainPath)
        {
            GameObject pathway = Instantiate(pathwayPrefab, branch.position, branch.rotation);
            pathway.tag = "Pathway"; 
            pathways.Add(pathway);
            //Debug.Log($"Pathway created at: {branch.position} with tag: {pathway.tag}");
        }

        List<Transform> unvisitedBranches = new List<Transform>();
        foreach (Transform branch in branches)
        {
            if (!mainPath.Contains(branch))
            {
                unvisitedBranches.Add(branch);
            }
        }

        //Debug.Log($"Unvisited branches count: {unvisitedBranches.Count}");

        Transform middleBranch = GetMiddleBranch(branches);
        if (middleBranch != null && !mainPath.Contains(middleBranch))
        {
            GameObject blockedBranch = Instantiate(blockedBranchPrefab, middleBranch.position, middleBranch.rotation);
            blockedBranch.tag = "BlockedBranch";
            blockedBranches.Add(blockedBranch);
            unvisitedBranches.Remove(middleBranch);
            //Debug.Log($"Middle blocked branch placed at: {middleBranch.position}");
        }

        int blockedCount = 0;
        while (blockedCount < minBlockedBranches && unvisitedBranches.Count > 0)
        {
            int index = Random.Range(0, unvisitedBranches.Count);
            Transform branch = unvisitedBranches[index];
            unvisitedBranches.RemoveAt(index);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(branch.position, 0.1f);
            bool overlapsWithPathway = false;
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Pathway"))
                {
                    overlapsWithPathway = true;
                    break;
                }
            }

            if (!overlapsWithPathway)
            {
                GameObject blockedBranch = Instantiate(blockedBranchPrefab, branch.position, branch.rotation);
                if (blockedBranch != null)
                {
                    blockedBranch.tag = "BlockedBranch";
                    blockedBranches.Add(blockedBranch);
                    blockedCount++;
                    // Debug.Log($"Blocked branch {blockedCount} placed at position: {branch.position} with tag: {blockedBranch.tag}"); // extensive debugging
                }
                //else
                //{
                    //Debug.LogError("Blocked branch prefab is null."); // comment out if no debug
                //}
            }
        }

        //Debug.Log($"Total blocked branches placed: {blockedCount}");
        RemoveIntersectingPathways();
    }

    public void AddExtraBlockedBranches(List<Transform> branches)
    {
        List<Transform> pathwayBranches = new List<Transform>();
        foreach (Transform branch in branches)
        {
            if (branch.CompareTag("Pathway"))
            {
                pathwayBranches.Add(branch);
            }
        }

        int blockedCount = 0;
        while (blockedCount < extraBlockedBranches && pathwayBranches.Count > 0)
        {
            int index = Random.Range(0, pathwayBranches.Count);
            Transform branch = pathwayBranches[index];
            pathwayBranches.RemoveAt(index); // remove pathway, make room for blockedd

            GameObject blockedBranch = Instantiate(blockedBranchPrefab, branch.position, branch.rotation);
            if (blockedBranch != null)
            {
                blockedBranch.tag = "BlockedBranch";
                blockedBranches.Add(blockedBranch);
                blockedCount++;
                Destroy(branch.gameObject);
                //Debug.Log($"Blocked branch {blockedCount} placed at position: {branch.position} with tag: {blockedBranch.tag}"); // ahhhhh ruining everything
            }
            //else
            //{
            //    Debug.LogError("Blocked branch prefab is null."); // god forbid
            //}
        }

        //Debug.Log($"Total extra blocked branches placed: {blockedCount}");
        RemoveIntersectingPathways();
    }

    private void CreatePathDFS(Transform currentBranch, Transform lowestBranch, HashSet<Transform> mainPath, List<Transform> branches)
    {
        if (currentBranch == lowestBranch)
        {
            return;
        }

        List<Transform> neighbors = GetNeighbors(currentBranch, branches);
        Shuffle(neighbors);

        foreach (Transform neighbor in neighbors)
        {
            if (!mainPath.Contains(neighbor))
            {
                mainPath.Add(neighbor);
                CreatePathDFS(neighbor, lowestBranch, mainPath, branches);
                if (mainPath.Contains(lowestBranch))
                {
                    break;
                }
            }
        }
    }

    private Transform GetHighestBranch(List<Transform> branches) // teleport player to top pathway initially
    {
        Transform highestBranch = null;
        float highestY = float.MinValue;
        foreach (Transform branch in branches)
        {
            if (branch.position.y > highestY)
            {
                highestY = branch.position.y;
                highestBranch = branch;
            }
        }
        return highestBranch;
    }

    private Transform GetLowestBranch(List<Transform> branches) // do not allow bottom branch to be blocked
    {
        Transform lowestBranch = null;
        float lowestY = float.MaxValue;
        foreach (Transform branch in branches)
        {
            if (branch.position.y < lowestY) // detect with y position
            {
                lowestY = branch.position.y;
                lowestBranch = branch;
            }
        }
        return lowestBranch;
    }

    private Transform GetMiddleBranch(List<Transform> branches) // this does not work very well. fixing
    {
        Transform middleBranch = null;
        float middleY = (GetHighestBranch(branches).position.y + GetLowestBranch(branches).position.y) / 2;
        float closestDistance = float.MaxValue;

        foreach (Transform branch in branches)
        {
            float distance = Mathf.Abs(branch.position.y - middleY);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                middleBranch = branch;
            }
        }

        return middleBranch;
    }

    private List<Transform> GetNeighbors(Transform branch, List<Transform> branches) // neighbors count as branches!
    {
        List<Transform> neighbors = new List<Transform>();
        foreach (Transform b in branches)
        {
            if (Vector3.Distance(branch.position, b.position) < 1.5f && branch != b)
            {
                neighbors.Add(b);
            }
        }
        return neighbors;
    }

    private void Shuffle<T>(List<T> list) // point, shuffle branches (pathway, blocked)
    {
        int n = list.Count; 
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void RemoveIntersectingPathways()
    {
        List<GameObject> pathwaysToRemove = new List<GameObject>();
        foreach (GameObject blockedBranch in blockedBranches)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(blockedBranch.transform.position, 0.1f);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Pathway"))
                {
                    pathwaysToRemove.Add(collider.gameObject);
                }
            }
        }

        foreach (GameObject pathway in pathwaysToRemove)
        {
            pathways.Remove(pathway);
            Destroy(pathway);
            //Debug.Log($"Pathway at position {pathway.transform.position} removed due to intersection with blocked branch");
        }
    }

    void FixedUpdate()
    {
        // if touching, remove
        RemoveIntersectingPathways();
    }
}
