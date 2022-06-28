using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int mazeSize = DontDestroy.size;
        int[,] testMaze = designMaze(mazeSize, mazeSize, 1);
        createMaze(testMaze);   
        GameObject mazeData = GameObject.Find("MazeSize");
        Destroy(mazeData);
    }

    // Struct to make coordinate data easier to manage
    public struct Point
    {
        public int x; // x-axis
        public int y; // y-axis
    }

    // Random number variable to be used repeatedly
    System.Random rand = new System.Random();

    // Size of the block component that makes up the maze
    const float blockX = 5f;
    const float blockY = 5f;
    const float blockZ = 5f;

    // Initial position in maze
    const float positX = 2.5f;
    const float positY = 2.5f;
    const float positZ = 2.5f;

    // Main Functions

    // Designs the maze behind the scenes
    // Returns the designed maze
    // x is maze width
    // y is maze length
    // d is maze difficulty
    
    int[,] designMaze(int x, int y, int d)
    {
        // Initialization of the maze
        Point start = startPoint(x, y);
        Point end = endPoint(x, y, start);
        int numDeadEnds = deadEnds(x, y, d);
        Point[] deadEndsLocations = deadEndPoints(x, y, numDeadEnds, start, end);
        int[,] maze = initializeMaze(x, y, start, end, deadEndsLocations);

        // Create the connecting paths between start, end, and dead ends
        List<Point> startToEnd = pathMaker(maze, start, end); // path from start to end
        List<Point>[] startToDeadEnds = new List<Point>[numDeadEnds]; // array contains the paths from start to dead ends
        int currentSize = 0; // How filled up startToDeadEnds is
        for (int i = 0; i < numDeadEnds; ++i)
        {
            if (i == 0)
            {
                startToDeadEnds[i] = pathMaker(maze, start, deadEndsLocations[i]);
                currentSize++;
            }
            else if (i > 0)
            {
                startToDeadEnds[i] = pathMaker(maze, pointChooser(startToDeadEnds, currentSize), deadEndsLocations[i]);
                currentSize++;
            }
        }
        int[,] definedPaths = new int[maze.GetLength(0), maze.GetLength(1)];
        for (int i = 0; i < startToEnd.Count; ++i)
        {
            definedPaths[startToEnd[i].x, startToEnd[i].y] = 1;
        }
        for (int i = 0; i < startToDeadEnds.Length; ++i)
        {
            for (int j = 0; j < startToDeadEnds[i].Count; ++j)
            {
                definedPaths[startToDeadEnds[i][j].x, startToDeadEnds[i][j].y] = 1;
            }
        }
        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {
                if (definedPaths[i, j] == 0) // Point that hasn't been set yet
                {
                    maze[i, j] = 4; // Try to make it a wall
                }
            }
        }
        return maze;
    }
    
    // Builds the maze visually in Unity
    // maze is the designed maze
    void createMaze(int[,] maze)
    {
        for (int i = 0; i < maze.GetLength(0); ++i)
        {
            for (int j = 0; j < maze.GetLength(1); ++j)
            {
                float xPosition = positX + (j * blockX);
                float zPosition = positZ + (i * blockZ);
                if (maze[i, j] == 4) // Point is a wall
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(blockX, blockY, blockZ);
                    wall.transform.position = new Vector3(xPosition, positY, zPosition);
                }
                if (maze[i, j] == 1) // Point is the start
                {
                    // Sets red cube as start point
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(1, 1, 1);
                    wall.transform.position = new Vector3(xPosition + 1, positY - 1, zPosition + 1);
                    wall.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                    // Sets the player at the start point
                    GameObject playableCharacter = GameObject.Find("FPSController");
                    playableCharacter.SetActive(false);
                    playableCharacter.transform.position = new Vector3(xPosition, positY, zPosition);
                    playableCharacter.SetActive(true);
                }
                if (maze[i, j] == 2) // Point is the end
                {
                    // Sets green cube as end point
                    GameObject end = GameObject.Find("EndPoint");
                    end.transform.position = new Vector3(xPosition, positY - 1, zPosition);
                }
                // Creating the walls around the maze
                if (i == 0) // top walls
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(blockX, blockY, blockZ);
                    wall.transform.position = new Vector3(xPosition, positY, zPosition - 5);
                }
                if (i == (maze.GetLength(0) - 1)) // bottom walls
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(blockX, blockY, blockZ);
                    wall.transform.position = new Vector3(xPosition, positY, zPosition + 5);
                }
                if (j == 0) // left walls
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(blockX, blockY, blockZ);
                    wall.transform.position = new Vector3(xPosition - 5, positY, zPosition);
                }
                if (j == (maze.GetLength(1) - 1)) // right walls
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(blockX, blockY, blockZ);
                    wall.transform.position = new Vector3(xPosition + 5, positY, zPosition);
                }
            }
        }
    }
    
    // Helper Functions

    // Returns an initialized maze with start, end, and dead end points
    // x & y are the maze's size
    int[,] initializeMaze(int x, int y, Point start, Point end, Point[] deadEnds)
    {
        int[,] maze = new int[x, y];
        maze[start.x, start.y] = 1; // 1 represents startPoint
        maze[end.x, end.y] = 2; // 2 represents endPoint
        for (int i = 0; i < deadEnds.Length; ++i)
        {
            maze[deadEnds[i].x, deadEnds[i].y] = 3; // 3 represents deadEnds
        }
        return maze;
    }
    
    // Returns the number of dead ends in the maze
    // x & y are the size of the maze; d is the difficulty
    int deadEnds(int x, int y, int d)
    {
        // Need to come up with complex function later through testing
        // May need to consider edge case of super small maze (1x1, 1x2, etc)
        int deadEnds = 0;
        if (x < 10 || y < 10)
        {
            deadEnds = 1;
        }
        else if (x < 100 || y < 100)
        {
            deadEnds = 10; // 5, 10 for 50x50      5 run speed, 10 jump speed
        }
        else
        {
            deadEnds = 30;
        }
        return deadEnds;
    }
    
    // Returns the randomized dead end locations
    // x and y are the size of the maze; n is number of dead ends
    // start and end points to avoid duplicate spots being chosen
    Point[] deadEndPoints(int x, int y, int n, Point startPoint, Point endPoint)
    {
        Point[] deadEndsLocation = new Point[n];
        for (int i = 0; i < n; ++i)
        {
            deadEndsLocation[i].x = rand.Next(0, x);
            deadEndsLocation[i].y = rand.Next(0, y);
            while ((startPoint.x == deadEndsLocation[i].x && startPoint.y == deadEndsLocation[i].y) 
            || (endPoint.x == deadEndsLocation[i].x && endPoint.y == deadEndsLocation[i].y))
            {
                deadEndsLocation[i].x = rand.Next(0, x);
                deadEndsLocation[i].y = rand.Next(0, y);
            }
        }
        return deadEndsLocation;
    }
    
    // Returns the randomized start position in the maze
    // x & y are the maze's size
    Point startPoint(int x, int y)
    {
        Point coord;
        coord.x = rand.Next(0, x);
        coord.y = rand.Next(0, y);
        return coord;
    }

    // Returns the randomized end position in the maze
    // x & y are maze's size
    // startPoint to avoid duplicate spots being chosen
    Point endPoint(int x, int y, Point startPoint)
    {
        Point coord;
        coord.x = rand.Next(0, x);
        coord.y = rand.Next(0, y);
        while (startPoint.x == coord.x && startPoint.y == coord.y)
        {
            coord.x = rand.Next(0, x);
            coord.y = rand.Next(0, y);
        }
        return coord;
    }
    
    // Returns a list of the points that make the path between start and end
    // maze is the initialized maze
    // start and end are the points a path is being created for
    List<Point> pathMaker(int[,] maze, Point start, Point end)
    {   
        int[,] visited = new int[maze.GetLength(0), maze.GetLength(1)];
        const int dist = 5; // Strictly vertical or horizontal distance between 2 points 
        Point curr;
        curr.x = start.x;
        curr.y = start.y;
        visited[curr.x, curr.y] = 1; 
        List<Point> builtPath = new List<Point>();
        builtPath.Add(curr);
        while (visited[end.x, end.y] != 1)
        {
            if (curr.x > end.x && curr.y < end.y) // end point is right and up from curr
            {
                int choice = rand.Next(0, 2);
                if (choice == 0 && isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1) // go right
                {
                    curr.y = curr.y + 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }
                else if (choice == 1 && isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1) // go up
                {
                    curr.x = curr.x - 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }                    
            }
            else if (curr.x < end.x && curr.y < end.y) // end point is right and down from curr
            {
                int choice = rand.Next(0, 2);
                if (choice == 0 && isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1) // go right
                {
                    curr.y = curr.y + 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }
                else if (choice == 1 && isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1) // go down
                {
                    curr.x = curr.x + 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }                 
            }
            else if (curr.x > end.x && curr.y > end.y) // end point is left and up from curr
            {
                int choice = rand.Next(0, 2);
                if (choice == 0 && isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1) // go left
                {
                    curr.y = curr.y - 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }
                else if (choice == 1 && isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1) // go up
                {
                    curr.x = curr.x - 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }              
            }
            else if (curr.x < end.x && curr.y > end.y) // end point is left and down from curr
            {
                int choice = rand.Next(0, 2);
                if (choice == 0 && isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1) // go left
                {
                    curr.y = curr.y - 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }
                else if (choice == 1 && isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1) // go down
                {
                    curr.x = curr.x + 1;
                    visited[curr.x, curr.y] = 1;
                    builtPath.Add(curr);
                }              
            }
            else if (curr.y < end.y) // end is right from curr
            {
                if (end.y - curr.y < dist) // just go right
                {
                    if (isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1)
                    {
                        curr.y = curr.y + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                }
                else
                {
                    int choice = rand.Next(0, 3);
                    if (choice == 0 && isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1) // go right
                    {
                        curr.y = curr.y + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                    else if (choice == 1 && isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1) // go up
                    {
                        curr.x = curr.x - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }        
                    else if (choice == 2 && isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1) // go down
                    {
                        curr.x = curr.x + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }           
                }
            }
            else if (curr.y > end.y) // end is left from curr
            {
                if (curr.y - end.y < dist) // just go left
                {
                    if (isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1)
                    {
                        curr.y = curr.y - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                }
                else
                {
                    int choice = rand.Next(0, 3);
                    if (choice == 0 && isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1) // go left
                    {
                        curr.y = curr.y - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                    else if (choice == 1 && isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1) // go up
                    {
                        curr.x = curr.x - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }        
                    else if (choice == 2 && isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1) // go down
                    {
                        curr.x = curr.x + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }           
                }
            }
            else if (curr.x > end.x) // end is up from curr
            {
                if (curr.x - end.x < dist) // just go up
                {
                    if (isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1)
                    {
                        curr.x = curr.x - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                }
                else
                {
                    int choice = rand.Next(0, 3);
                    if (choice == 0 && isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1) // go left
                    {
                        curr.y = curr.y - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                    else if (choice == 1 && isValidPoint(curr.x - 1, curr.y, maze) && visited[curr.x - 1, curr.y] != 1) // go up
                    {
                        curr.x = curr.x - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }        
                    if (choice == 2 && isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1) // go right
                    {
                        curr.y = curr.y + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }      
                }
            }
            else if (curr.x < end.x) // end is down from curr
            {
                if (end.x - curr.x < dist) // just go down
                {
                    if (isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1)
                    {
                        curr.x = curr.x + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                }
                else
                {
                    int choice = rand.Next(0, 3);
                    if (choice == 0 && isValidPoint(curr.x, curr.y - 1, maze) && visited[curr.x, curr.y - 1] != 1) // go left
                    {
                        curr.y = curr.y - 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }
                    else if (choice == 1 && isValidPoint(curr.x + 1, curr.y, maze) && visited[curr.x + 1, curr.y] != 1) // go up
                    {
                        curr.x = curr.x + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }        
                    if (choice == 2 && isValidPoint(curr.x, curr.y + 1, maze) && visited[curr.x, curr.y + 1] != 1) // go right
                    {
                        curr.y = curr.y + 1;
                        visited[curr.x, curr.y] = 1;
                        builtPath.Add(curr);
                    }      
                }
            }
        }
        return builtPath;
    }  

    // Determines if a point is in the maze
    // x & y represent the point's coordinates
    // maze is the current maze
    bool isValidPoint(int x, int y, int[,] maze)
    {
        if (x < maze.GetLength(0) && y < maze.GetLength(1) && x > -1 && y > -1)
        {
            return true;
        }
        return false;
    }

    // Returns a random point chosen from a random path
    Point pointChooser(List<Point>[] paths, int size)
    {
        int randPath = rand.Next(0, size); // Choose the random path
        int randPoint = rand.Next(0, paths[randPath].Count); // Choose the random point in the chosen path
        return paths[randPath][randPoint];
    }
}