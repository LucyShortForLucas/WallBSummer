# PART 0 - BASICS

This is the techdoc for our submission to the course Group Projects 2026. This document stipulates the standards and conventions we will be using throughout the project. This document is written and maintained by me, Lucy, in accordance with our team's standards. 

This is a living document and may grow, shrink, or evolve as our team and project does the same. All team members are expected to be up to date with its standards. 

# Summary

Wall-B (Working title) is a single-player top-down 3d base-builder and resource manager in which the player, a small automaton, protects and nurtures a growing patch of fertile land. To accomplish this, they must gather water to fuel the growing plants as well as themselves and their machines, and build such machines and buildings to extract and refine resources and protect their base from evil drones and automatons that want to destroy your plants and steal your water. 

As the player's base grows, they attract more and stronger enemies. Those enemies drop 'scrap', the main resource used to build more buildings. At the same time the player must ensure that they build their base in harmony with nature, and allow for space for their growing land to spread. 

# Development Stack 

* Unity 6.5 (6000.5.4f1) 
    * C#, .NET standard 2.1 (default Unity .NET API) 
    * C++ 20 Native Unity Plugins
    * New Input system 
    * uGUI (Unity UI)
* Visual Studio 2026 
* Wwise
* ...add as needed

## Unity 6.5 LTS (6000.5.4f1) 

This project will be using the Unity Game Engine. Unity allows for faster iteration on ideas and a smoother development cycle. C# as a high-level scripting language is both simpler and safer than Unreal C++.

### .NET Standard 2.1

Unity supports two .NET API compatability levels: .NET Standard 2.1 and the .NET Framework 4.8. This project will use the default API compatibility level, as it creates smaller executables and provides better cross-platform support. We currently have no need for any functionality from the .NET Framework 4.8.

### New Input System

This project exclusively uses the new Input System to detect and react to user input. No other or legacy systems should be used for input handling.

### uGUI (Unity UI)

This project uses the legacy uGUI UI system. We will not use the new UI Toolkit. The legacy UI system is gameobject-based and very closely resembles the default Gameobject/component workflow that gameplay developers are used to.

The new UI Toolkit, while more powerful, is UXML-based and more closely resembles regular GUI/web-based development. No one in our team has proper experience with XML-based application development or the UI toolkit, so we will stick to the legacy uGUI system.

## Visual Studio 2026

Programmers will use Visual Studio 2026 as their IDE of choice to write C# and C++ code.

<br></br>

# PART 1 - PROGRAMMING GUIDELINES

Below follow a number of programming guidelines. When writing code in the project, stick to these guidelines. For the most part, they should be considered hard rules. These rules exist to keep code in the project consistent, clean, and well-written.
 
When reviewing code from yourself and others, flag violations of these rules when they occur. If you encounter a problem with some code in the project, but cannot find a guideline below to flag it with, call it out and consider proposing a new guideline to add.

Each rule has a 3-part identifier, seperated by periods. The first part being the language the rule is about, the second being the category within that language, and finally a number to denote the how-manieth guideline of that category it is. When flagging a rule violation in a code review, call out rules by this identifier, no need to write out the full name.

# Cs - C# guidelines

For this project we will be using C# and .NET Standard 2.1. As such, unless specified otherwise in this document, assume that the official Microsoft C# guidelines apply. They can be found here: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

## Cs.N - Naming conventions

This project will be using Microsoft's official C# naming conventions for identifiers. They can be found here: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names

## Cs.I - Indentation

### Cs.I.1 - Opening braces go on on their own line if the code block spans more than one line

* **Good**:
    ```C#
    public void MyMethod(bool myParam)
    {
        if (myParam)
        {
            // Execute some code...
            // ...
        }
        else
        {
            // Execute some code...
            // ...
        }
    }
    ```
* **Bad**:
    ```C#
    public void MyMethod(bool myParam) {
        if (myParam) {
            // Execute some code...
            // ...
        }
        else {
            // Execute some other code...
            // ...
        }
    }
    ```

**REASON:** Code clarity and consistency.

### Cs.I.2 - If a code block is only a single line, braces may be left out and the statement itself may go on the next line

* **Without braces**:
    ```C#
        if (myParam)
            // Execute some code...
        else
            // Execute some other code...
    ```
* **With braces**:
    ```C#
        if (myParam) 
        {
            // Execute some code...
        }
        else 
        {
            // Execute some code...
        }
    ```

**REASON:** Clarity and brevity. Using braces for a single line doesn't necessarily aid with readability. Using braces here is still allowed, but not required. Use your best judgement.

### Cs.I.3 - If you only have one or simple attribute(s) attached to an identifier, it goes on the same line as the identifier.

* **Good**:
    ```C#
    [SerializeField] private int _mySerializedField;
    [SerializeField] private int _myOtherSerializedField;
    [SerializeField] private int _aThirdSerializedField;
    ```

* **Bad**:
    ```C#
    [SerializeField] 
    private int _mySerializedField;
    [SerializeField] 
    private int _myOtherSerializedField;
    [SerializeField] 
    private int _aThirdSerializedField;
    ```

**REASON:** Clarity and readability. Treating simple attributes like other modifiers makes sense and keeps the code readable, especially when declaring multiple fields in a row.

### Cs.I.4 - If you have more than one attribute or complex attributes attached to an identifier, they should go on the previous line.

* **Good**:
    ```C#
    [SerializeField]
    [Tooltip("This variable decides the monetary value of this pickup.")]
    [Range(0, 10)]
    private int _pickupValue = 1f;
    ```

* **Bad**:
    ```C#
    [SerializeField] [Tooltip("This variable decides the monetary value of this pickup.")] [Range(0, 10)] private int _pickupValue = 1f;
    ```

**REASON:** Attributes such as tooltips can become too large, having an adverse effect on readability. When in doubt, use your best judgement.

## Cs.S - Structural conventions

Below follow some simple conventions regarding the structure of code and code files to keep them consistent, safe, and readable.

### Cs.S.1 - Avoid declaring public fields in a class

In a class, a field should always be private. If you must expose a field without an explicit getter or setter, simply expose it in a Property. Structs are allowed to have public fields, of course.

* **Good**:
    ```C#
    private int _someField = 5;
    public int SomeField { get => _someField; set => _someField = value; }
    ```
* **Bad**:
    ```C#
    public int SomeField = 5;
    ```

**REASON**: Wrapping public access to data in a property makes it both simpler and safer to add logic to getting or setting that date from outside the class later, while still allowing the class itself to bypass this logic if needed.

### Cs.S.2 - Avoid auto-implemented properties. 

In a class, properties should always explicitly refer to a private field. Even if the getter and setter are trivial, the private field should still be explicitly called out in code.

* **Good**:
    ```C#
    private int _someField = 5;
    public int SomeField { get => _someField; set => _someField = value; }
    ```
* **Bad**:
    ```C#
    public int SomeField { get; set; }
    ```

**REASON**: Wrapping public access to data in a property makes it both simpler and safer to add logic to getting or setting that data from outside the class later, while still allowing the class itself to bypass this logic if needed.

### Cs.S.3 - Always specify an access modifier. Do not rely on implicit 'private'

* **Good**:
    ```C#
    private int _someField = 5;
    ```
* **Bad**:
    ```C#
    int _someField = 5;
    ```

**REASON:** Being verbose and explicit with access modifiers avoids confusion and makes your intention clear. 

### Cs.S.4 - Do not use the null-coalescing operator (??) or the null-conditional operator (?.) with UnityEngine.Object or any class that derives from it.

When checking a UnityEngine.Object or any derived class of it, such as a monobehaviour, always use an explicit null-check instead.

* **Good**:
    ```C#
    if (_myNullableMonobehaviour != null)
        _myNullableMonobehaviour.CallSomeMethod();
    ```
* **Bad**:
    ```C#
    _myNullableMonobehaviour?.CallSomeMethod();
    ```

**REASON:** Unity overrides the ``==`` operator for ``UnityEngine.Object`` and ``null`` to return ``true`` when an object is marked for cleanup, even if the object's actual value is not yet actually ``null``. The ``??`` and ``?.`` operators check for real ``null`` and cannot be overriden, and may thus give false positives when used on any Unity object (this includes ScriptableObjects and Monobehaviours, and pretty much any other class created by Unity)

### Cs.S.5 - Prefer early returns over nested if-statements

* **Good**:
    ```C#
    if (!myBool) return;
    // Execute some code if myBool is true
    // ...
    if (!myOtherBool) return;
    // Execute some code if myBool and myOtherBool are true
    // ...
    ```
* **Bad**:
    ```C#
    if (myBool) 
    {
        // Execute some code if myBool is true
        // ...
        if (myOtherBool) 
        {
            // Execute some code if myBool and myOtherBool are true
            // ...
        }
    }
    ```

**REASON:** Deeply nested code is harder to read and harder to follow. Early return statements clearly communicate intent and allow code to be less nested.

**NOTE:** In loops, the same applies to ``Continue`` and ``Break``

## Cs.A - Architectural conventions

Below follow a list of conventions regarding the architecture, patterns, and constructs to use and avoid while working on this project.

### Cs.A.1 - Preface C# scripts with '#nullable enable' and use nullable types (T?) if a variable must be nullable

By adding the preprocessor instruction "#nullable enable" to the top of a C# file we turn the language's implicit nullability checks back on. This way, any field of a non-nullable type MUST have a non-null value when exiting the class's constructor, and it cannot be assigned null. For a field to be able to hold a null value it must be explicitly declared as a nullable type.

* **Good**:
    ```C#
    #nullable enable
    using UnityEngine;

    public class Example : MonoBehaviour
    {
        private string? _MyNullableString;
        //...
    }
    ```
* **Bad**:
    ```C#
    using UnityEngine;

    public class Example : MonoBehaviour
    {
        private string _MyNullableString;
        //... 
    }
    ```

**REASON:** With nullable enabled, non-nullable types promise to always hold a valid value and client code does not have to check for null. In the same vein, when attempting to access a field of a nullable type, Visual Studio will explicitly warn you if you forget to add a null check. 

### Cs.A.2 - Prefer C# Events and Delegates over UnityEvents

When utlizing the Observer pattern, always use C# events or delegates. Do not use UnityEvents for this purpose.

* **Good**:
    ```C#
    public class ExampleClass : MonoBehaviour
    {
        public event Action? MyEvent;

        private void OnEnabled()
        {
            MyEvent += OnEventTriggered;
        }

        private void OnDisabled()
        {
            MyEvent -= OnEventTriggered;
        }

        private void Update()
        {
            if (Input.anyKeyDown && MyEvent != null)
                MyEvent?.Invoke();
        }

        private void OnEventTriggered()
        {
            Debug.Log("Callback executed");
        }
    }
    ```
* **Bad**:
    ```C#
    public class ExampleClass : MonoBehaviour
    {
        public UnityEvent? _myEvent;

        void Start()
        {
            if (_myEvent == null)
                _myEvent = new UnityEvent();

            _myEvent.AddListener(OnEventTriggered);
        }

        void Update()
        {
            if (Input.anyKeyDown && _myEvent != null)
                _myEvent.Invoke();
        }

        void OnEventTriggered()
        {
            Debug.Log("Callback executed");
        }
    }
    ```

**REASON:** C# events are a language feature, much more performative, and expressed entirely in code. This makes them more testable, traceable, and easier to debug. UnityEvents also have the 'downside' of being First Class objects stored in variables. While this allows them to be hooked up in the editor, this is itself not much of a benefit as these relationships should be expressed in code, not the inspector, wherever possible.

**EXCEPTION:** Canvas elements like buttons ONLY work with UnityEvents. For this purpsose you are, naturally, allowed (read: forced) to use them.

### Cs.A.3 - A class should only have one responsibility (i.e. avoid God Objects)

In Object-oriented programming, a 'God object' or 'God class' is a class that has a lot of unrelated data and methods and carries responsibility over a wide range of unrelated things. It is easy to 'accidentally' create one as classes and systems grow; think of a basic 'Player' script, which might initially just handle the player's movement, but then have health, attacks, an inventory, etc. attached to it. 

A class should ideally never do two unrelated things. You should be able to, in a short paragraph, explain everything the class does. If a single class does do much, or grows too big, consider splitting it up into several distinct classes, or moving specific pieces of logic into helper classes

* **Good**:
    ```C#
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 5;

        private void Start() 
        {
            // Some startup logic for player movement
        }

        public void MovePlayer(Vector2 direction)
        {
            // ...
        }
    }

    public class Health : MonoBehaviour
    {
        [SerializeField] private float _maxHp = 100;
        private float _hp = _maxHp

        private void Start() 
        {
            // Some startup logic for Health
        }

        public void TakeDamage()
        {
            // ...
        }
    }

    // Etc...
    ```
* **Bad**:
    ```C#
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _speed = 5;
        [SerializeField] private float _maxHp = 100;
        [SerializeField] private float _weaponDmg = 10;

        private float _hp = _maxHp

        private void Start() 
        {
            // Some startup logic for player movement

            // Some startup logic for Health

            // Some Startup logic for attacks

            // etc..
        }

        public void MovePlayer(Vector2 direction)
        {
            // ...
        }

        public void TakeDamage()
        {
            // ...
        }

        public void AttackEnemy()
        {
            // ...
        }
    }
    ```

### Cs.A.4 - Avoid Singletons

A class is a singleton when it both enforces and ensures that exactly one instance of it exists at all times. In the classic pattern this is accomplished through a private constructor and a static reference to an instance of itself, which it constructs at startup; though there are other, equally undesirable, ways to implement it.

There are several ways to avoid using Singletons if you ever feel the need for one. Preferable alternatives are outlined in the ``Other`` section of this document below.

**IMPORTANT NOTE:** The word 'singleton' has been used and abused across the software industry for decades, and not always to refer to things that are actual Singletons. This convention is about the Singleton Pattern specifically, where a class enforces the existance of a single instance of itself and forbids others from instantiating more; not about any other (mis)use of the term.

* **Bad**:
    ```C#
    public class MySingleton
    {
        static MySingleton Instance = new();

        private MySingleton() 
        {
            // Construct an instance...
        }
    }
    ```

**REASON:** Singletons are basically complicated global objects. As with all global state, no single piece of code can ever reason about its state on its own. Objects that depend on a Singleton hide that dependency, and become untestable in closed environments. Unit testing them becomes impossible. Any object that depends on a Singleton is tightly coupled to it and at most ever a single degree of separation away from every other object. 

### Cs.A.5 - Do not inherit from Monobehaviour in a class if it does not need to be one.

Classes that inherit from Monobehaviour are treated as custom components intended to be added to GameObjects. If a class is not a component, it should not inherit from Monobehaviour. If the class needs to have serialized data and be viewable in the editor, it could be a ScriptableObjet instead, if it requires some simple Unity engine integration, it could be a UnityEngine.Object, and if it is simply a helper class for other scripts, it could be a plain C# class.

In general, a class should only be a monobehaviour if it requires one of Monobehaviour's specific methods, such as Update.

* **Good**:
    ```C#
    public class MyCsharpClass
    {
        // Helper methods to be used by other scripts
        // ...
    }
    ```

* **Bad**:
    ```C#
    public class MyCsharpClass : MonobBehaviour
    {
        // Helper methods to be used by other scripts
        // ...
    }
    ```

**REASON:** Monobehaviour can, at times, be overkill. Monobehaviours are specifically meant for scripting behaviour and components. If you can get away with a more lightweight base class or even no base class at all, your code will be simpler and more maintable.

<br></br>

# PART 2 - PROJECT STRUCTURE

# Unity Project Structure
This is the top-level folder structure of our Unity project. Below, individual parts of this structure are explained more in-depth below.
```
/Root/
    Assets/
        Game/
            Audio/
            Graphics/
            Systems/
            Input/
            Scenes/
        Editor/
            Scripts/
        Dev/
            Lucy/     ⎤
            Thommy/   ⎥  --> One dev folder per person
            Robbe/    ⎥      
            .../      ⎦
        TextMeshPro/
        Settings/
        Plugins/
    Packages/        ⎤  ---> Generated folders, 
    ProjectSettings/ ⎦       Included in Version Control 

    Library/ ⎤
    Logs/    ⎥ ---> Local Unity folders
    Temp/    ⎥      excluded from Version Control
    .../     ⎦
```
## Dev 
This is where we put any and all work-in-progress files and assets. Within this folder, every team member gets their own folder. Internally, these individual folders may be as (dis)organized as needed. Any assets within these folders are considered **temporary** and **should not be touched by anyone else.** As soon as a file or asset is finished, it should be moved to the appropriate location in **Game** or **Editor**. 

**No asset inside Dev may be depended on by any asset outside of it. This folder is NOT included in the main branch.**

## Editor
In this file, all editor-specific assets should go, such as tools and in-editor scripts.

## Game
This is the place where all **game-ready** assets should go. It is internally divided up into further categories, each with their own folder structure, explained below.

### systems
This folder will hold all of the **scripts** and **prefabs** used in the game. In simpler terms, this is the place where all of the code and their implementations go. Internally, they are split up into logical software **systems**.

A **System** in this sense is a single coherent unit of behaviour. **Every single script and prefab should be part of a larger system.** If you find yourself in need of writing a script or creating a prefab that does not fit any established system, this likely indicates a flaw in in your design or architecture. 

**Do not** add new system folders without discussing it with the team. **All systems** should be established and explained in **SystemDocs.md**.

All systems are further categorized into one of **four system categories**:

- **Core systems**: These are the most **fundamental** and low-level systems that other systems **depend on** and that the game cannot exist without. In general, a system is a core system if many other systems **cannot** reasonably exist or be **tested** without these. Examples of core systems are the grid, in-game resource, and health systems.

- **Gameplay systems**: These are the high-level gameplay systems that make up the bulk of the interactable player systems, but that themselves are **not** heavily depended on by other systems. Unlike with Core Systems, gameplay systems should be largely **orthogonal** to each other and be able to be **tested** independently. Removing one entirely should **not** leave other systems in an un-executable state. Examples include the build system and the farming system.

- **I/O systems**: These are all of the **Input/Output** and **data management** related systems. These are the systems that handle all of the communication to and fro the **user**, such as **input handling** and **UI** and the **OS**, such as **save systems**. Importantly, **NO system outside this category should do or care about any sort of I/O**.

- **Utility systems**: These are the additional and superfluous systems that are neither core to the game or part of the gameplay. Many of these will be **small, critical** systems that **facilitate other systems** or provide **additional utilities.** These are the systems such as our **dependency injection**.

## Settings
This folder holds URP settings.

## TextMeshPro
The main folder of the TextMeshPro package, holding fonts and assets related to the package.

# ArtAssets

In this folder we expect all **source art** assets that are **used in the game**. 
Ideally, the folder structure in this directory matches the folder structure of the game. That way it's easy to find a source file of an asset in the game.

It is not recommended to include export files (```.fbx```, ```.png```, etc..) in this folder, since Unity project already contains these files if the asset is imported. If we store export files in ArtAssets as well, we need to keep the files in both locations up-to-date. Hovewer, storing export files in ArtAssets folder is not forbitten.

# SoundAssets

In this folder we expect all **source sound** assets that are **used in the game**

This folder contains typically a reaper, FMod ore Wwise project along with the source files used to create the sound assets that are used in game.
Make sure that these projects have a proper .p4ignore file excluding all files that are not needed.

Do **not** use this folder during prototyping, this folder should only contain projects and assets that are part of the final product.

# File and Folder Naming conventions
These naming conventions are applicable to all files and folders in the project.

## Casing
The names for both files and folders should be in **PascalCase**.

- ✔️ Good: ```HelloKitty.psd```, ```HelloKitty.fbx```, ```HelloKitty.scene```
- ❌ Bad: ```hello_kitty.psd```, ```helloKitty.fbx```, ```Hello Kitty.scene```

## Special characters
File and folder names may only include upper-case and lower-case alphanumeric characters (A-B, a-b, 0-9) and underscores (_), and they may not start with a number or underscore.

- ✔️ Good: ```FiveNightsAtFreddys```, 
- ❌ Bad: ```FiveNightsAtFreddy's```, ```Five Nights at Freddys```

## Dates
Dates should be written in the following format: ``DD.MM.YYYY``

- ✔️ Good: ```23.03.2026```
- ❌ Bad: ```23.3.2026```, ```23/03/2026```, ```23.03.26```

## Prefixes

We do not use prefixes. When naming files, like materials, models and textures, we do not use any prefixes to define what 'type' of asset it is (No: ```T_``` or ```SM_```). File type should be clear from the extension in the file explorer and the folder structure they are a part of.

- ✔️ Good: ```Rocket_BC.png```, ```Rocket.fbx```
- ❌ Bad: ```T_Rocket_BC.png```, ```SM_Rocket.fbx```

## Suffixes (a.k.a. Postfixes)

We use suffixes only for the files and only for the ones in the list below. Suffixes are written after an underscore ("_").

The list of available suffixes:

* Names: ```ArtBibleSketches_Alice.psd```, ```UnityPractice_Bob```
* Textures:
    * ``_BC``: Base color texture
    * ``_N``: Normal map
    * ``_ORM``: Channel-packed texture: Red - Ambient Occlusion | Green - Roughness | Blue - Metallness
    * ``_OGM``: Channel-packed texture: Red - Ambient Occlusion | Green - Glosiness | Blue - Metallness
    * ``_E``: Emissive texture
    * ``_Mask``: Single-channel grayscale mask
    * ``_Masks``: Channel-packed texture where every channel is a grayscale mask
    * If there is a file with another texture type, then file should be abbreviated in the similar way as in this list.
* Meshes:
    * ``_SKM``: Skeletal Mesh
    * ``_SK``: Skeleton
    * No suffixes are used for the static meshes.

## Variations & Versions
If a file has multiple variants or versions, this should be denoted in the name using a number at the end. This number should have at least 2 digits, i.e. have a leading zero for single digit numbers, for clarity.

Example:
- ✔️ Good: ``Rock01.fbx``, ``Rock102_BC.png``
- ❌ Bad: ``Rock1.fbx``, ``Rock_102_BC.png``