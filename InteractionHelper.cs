using System;
using System.IO;

using UnityEngine;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;

// Asynvhronous programming in .NET development
using System.Net.Http;
using System.Threading.Tasks;

public class InteractionHelper : MonoBehaviour
{
    // -----------------------------
    // Config Parameters
    // -----------------------------

    // TextMesh is attached to a hololens unity prefab which displays text in virtual environments
    public TextMesh _debugText;
    // These are two different material also configured in unity engine, allowing the user to know which object they are looking at
    public Material MaterialInGaze;
    public Material _oldMaterial;
    // The model of the brick
    public GameObject brick_prefab;
    // Defined unity methods
    private GestureRecognizer tapRecognizer;
    private Rigidbody rb;
    // Custom defined gameobjects, stings and bools used for object oriented programming
    private GameObject last_hit;
    private GameObject current_hit;
    private GameObject active_brick;
    private GameObject freeze_object;
    private GameObject[] bricks = new GameObject[100];
    private String positionString;
    private bool odd_tap;

    // counters for counting the index of new bricks
    private int count = 0;
    private int previousCount = 0;

    // initiate the position and rotation data of last brick added
    private Vector3 lastBrickPosition;
    private Vector3 lastBrickRotation;

    // initiate the 6 coordinates' data
    private string posX;
    private string posY;
    private string posZ;
    private string rotX;
    private string rotY;
    private string rotZ;

    // initiate the path the data will be written into
    private static readonly HttpClient client = new HttpClient();

    // initiate the path the data will be written into
    public string path;

    // -----------------------------
    // Cached References
    // -----------------------------

    private NetworkingHelper networking;
    private WriteTextHelper writeTextHelper;

    // -----------------------------
    // Main Loop
    // -----------------------------

    // Runs at the start of the application
    void Start()
    {
        // Setting object reference to the instance of object
        networking = new NetworkingHelper();
        writeTextHelper = new WriteTextHelper();

        // Sends starting message to server to check whether the http functions are working
        StartCoroutine(UploadHTTP("Connected!"));

        // Starts clock for updating current brick coordinate text mesh in virtual environment at a slow 2fps to reduce computation
        InvokeRepeating("UpdateText", 0.5f, 0.5f);

        // initially assign to empty string
        posX = "";
        posY = "";
        posZ = "";
        rotX = "";
        rotY = "";
        rotZ = "";

        // Initializes gesture recognition for single taps
        tapRecognizer = new GestureRecognizer();
        tapRecognizer.SetRecognizableGestures(GestureSettings.Tap);
        tapRecognizer.TappedEvent += TapRecognizer_TappedEvent;
        tapRecognizer.StartCapturingGestures();

        odd_tap = true;// Used for identifying odd and even taps, which either creates a new brick or dropps a brick

        // Voice Recognition for resetting brick
        KeywordRecognizer resetRecognizer =
            new KeywordRecognizer(new[] { "Reset" });
        resetRecognizer.OnPhraseRecognized += ResetRecognizer_OnPhraseRecognized;
        resetRecognizer.Start();

        // assign the file path the data will be written into
        //path = "Asset/Resources/test.txt"; // relative path
        path = "C:/Users/Danny/Documents/RobotSandbox/test.txt"; // absolute path: so the path could be referred to after the compilation

        // Clear the content of text file before loading data into it
        File.WriteAllText(path, String.Empty);
    }

    // Runs at very fast fixed intervals for smoothness
    void FixedUpdate()
    {
        // Set distance (m) of the active brick in front of user's camera when in control
        float distance = 1.5f;

        // Sets the movement path for the brick, using the origin position and forward vector of the camera
        if (active_brick != null)
        {
            Rigidbody rb = active_brick.GetComponent<Rigidbody>();
            rb.MovePosition(Camera.main.transform.position + Camera.main.transform.forward * distance);
        }
    }

    // Runs at 30-60fps depending on performance
    void Update()
    {
        // Creating a line vector starting from the origin of the main camera forward, simulating viewer's sight (Gaze)
        var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit raycastInfo;

        // Returns object that is hit by this line and sends a message
        if (Physics.Raycast(ray, out raycastInfo))
        {
            current_hit = raycastInfo.transform.gameObject;
            if (current_hit == last_hit) // Terminates operation if object in gaze hasn't changed to reduce computation
                return;

            // If object in gaze has changed, assign its mesh renderer component of object under renderer
            var renderer = current_hit.GetComponent<Renderer>();
            // some objects don't have mesh renderers, checking for error
            if (renderer == null)
                return;

            // If there was an object before this new object in gaze, change that object's material back to it's original material
            if (last_hit != null)
            {
                var last_render = last_hit.GetComponent<Renderer>();
                last_render.material = _oldMaterial;
            }

            // Don't do anything when the object in gaze is part of workspace (these are the invisible walls that shouldn't interact with the gaze)
            if (current_hit.tag == "workspace")
                return;
            
            // Set old Material to the current mesh renderer material
            _oldMaterial = renderer.material;
            // Set the current mesh renderer material to a custom material in unity called MaterialInGaze
            renderer.material = MaterialInGaze;
            // Repeat process
            last_hit = current_hit;
        }
        // If nothing is hit, but there was an object in gaze in the previous frame, reset previous object's material
        else
        {
            if (current_hit == null)
                return;
            if (last_hit == null)
                return;
            var renderer = last_hit.GetComponent<Renderer>();
            renderer.material = _oldMaterial;
            current_hit = null;
        }
    }

    // -----------------------------
    // Customised Methods
    // -----------------------------

    // Function for freezing both rotation and position of brick, before brick coordinates are sent.
    private void _freeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
        // make the current brick count equal the previous brick count for next iteration
        previousCount = count;
        tapRecognizer.StartCapturingGestures();
    }

    // Tap event listener
    private void TapRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        // Drops brick on even taps
        if (odd_tap == false)
        {
            // Stops registering tap events to provent spawning new bricks before server message is sent
            tapRecognizer.StopCapturingGestures();
            // Changes property of active_brick to freeze_object so it nolonger follows the camera
            freeze_object = active_brick;
            active_brick = null;
            // Accesses the rigid body components of the freeze_object game object and enables gravity so object falls
            rb = freeze_object.GetComponent<Rigidbody>();
            rb.useGravity = true;
            // Set odd_tap to true so next tap spawns a new brick
            odd_tap = true;
            // Executes custom _freeze method 0.5 seconds after fall, so it has plenty of time to reach the ground
            Invoke("_freeze", 0.5f);
            // Posts the position and rotation information to the server
            networking.MainAsync(
                posX,
                posY,
                posZ,
                rotX,
                rotY,
                rotZ);
            // Write the position and rotation information into local text file
            writeTextHelper.WriteString(
                posX,
                posY,
                posZ,
                rotX,
                rotY,
                rotZ);
        }
        // Creates brick on odd taps
        else
        {
            // Initializes brick model to starting position and default rotation
            var newBrickPosition = new Vector3(0f, 0.65f, -2f);
            GameObject NewBrick = Instantiate(brick_prefab, newBrickPosition, Quaternion.identity);
            // Tags the brick for easy grouping
            NewBrick.tag = "brick";
            var rb = NewBrick.GetComponent<Rigidbody>();
            // Sets rigid body properties for the brick while it's controlled by viewer's camera
            if (rb == null)
                return;
            // Removes the effect of gravity
            rb.useGravity = false;
            // Removes velocity
            rb.velocity = Vector3.zero;
            // Set desired brick rotation
            rb.rotation = Quaternion.Euler(-90f, 0f, 0f);
            // Fixes the rotation and only allow translation
            rb.freezeRotation = true;
            // Assigning counts to the bricks array, to make it easier to access the most recent brick.
            bricks[count] = NewBrick;
            count += 1;
            // Sets NewBrick as active_brick which follows the camera movements
            active_brick = NewBrick;
            odd_tap = false;
        }
    }

    // Voice input listener for resetting brick
    private void ResetRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (active_brick != null)
            return;
        if (freeze_object == null)
            return;
        // Resets brick's rigidbody properties
        var rb = freeze_object.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        // Resets brick's position
        freeze_object.transform.position = new Vector3(0f, 0.65f, -0.5f);
        // Reactives brick to follow camera position
        freeze_object = active_brick;
    }

    // Updates coordinates printed within virtual environment
    public void UpdateText()
    {
        // Indicates 1 new brick added to the scene when 'count' is bigger than 'previousCount' by 1
        // Use this logic to prevent the update() function from writing coordinates all the time
        if (count != previousCount)
        {
            lastBrickPosition = bricks[count - 1].transform.position;
            lastBrickRotation = bricks[count - 1].transform.rotation.eulerAngles; // eulerAngles is the conversion from quaternion to Vector3

            // Set the string objects to corresponding transformation variables
            posX = lastBrickPosition.x.ToString("F3");
            posY = lastBrickPosition.y.ToString("F3");
            posZ = lastBrickPosition.z.ToString("F3");
            rotX = lastBrickRotation.x.ToString("F3");
            rotY = lastBrickRotation.y.ToString("F3");
            rotZ = lastBrickRotation.z.ToString("F3");
            
            // Assigns the position and rotation information under positionString
            positionString =
                posX + "," +
                posY + "," +
                posZ + "," +
                rotX + "," +
                rotY + "," +
                rotZ + ",";
            // Shows this position on _debugText which is shown in the virtual environment
            _debugText.text = positionString;

        }       
    }           
}               
                
                