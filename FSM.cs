using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace svblm.bootstrap {
  /// <summary>
  /// A base Finite State Machine class for building state machines.
  /// Define a number of states in an enum at the top of your file.
  /// Then define your delegate methods with the format State_Update()
  /// change the currentState property to transition states.
  /// </summary>
  public abstract class FSM : MonoBehaviour {

    public string state = "None";
    // Cache Monobahaviour components
    [HideInInspector]
      public new Transform transform;
    [HideInInspector]
      public new Collider collider;
    [HideInInspector]
      public new GameObject gameObject;
    [HideInInspector]
      public new Animation animation;
    [HideInInspector]
      public CharacterController controller;
    [HideInInspector]
      public new NetworkView networkView;
    [HideInInspector]
      public new Rigidbody rigidbody;

    void Awake() {
      transform = base.transform;
      collider = base.collider;
      gameObject = base.gameObject;
      animation = base.animation;
      rigidbody = base.rigidbody;
      networkView = base.networkView;
      controller = GetComponent<CharacterController>();
      OnAwake();
    }

    protected virtual void OnAwake() {
    }

    static IEnumerator DoNothingCoroutine() {
      yield break;
    }

    static void DoNothing() {
    }

    static void DoNothingCollider(Collider other) {
    }

    static void DoNothingCollision(Collision other) {
    }

    protected virtual void Always_BeforeUpdate() {

    }

    protected virtual void Always_AfterUpdate() {

    }

    protected virtual void Always_LateUpdate() {

    }

    protected virtual void Always_FixedUpdate() {

    }

    public Action DoUpdate = DoNothing;
    public Action DoLateUpdate = DoNothing;
    public Action DoFixedUpdate = DoNothing;
    public Action<Collider> DoOnTriggerEnter = DoNothingCollider;
    public Action<Collider> DoOnTriggerStay = DoNothingCollider;
    public Action<Collider> DoOnTriggerExit = DoNothingCollider;
    public Action<Collision> DoOnCollisionEnter = DoNothingCollision;
    public Action<Collision> DoOnCollisionStay = DoNothingCollision;
    public Action<Collision> DoOnCollisionExit = DoNothingCollision;
    public Action DoOnMouseEnter = DoNothing;
    public Action DoOnMouseUp = DoNothing;
    public Action DoOnMouseDown = DoNothing;
    public Action DoOnMouseOver = DoNothing;
    public Action DoOnMouseExit = DoNothing;
    public Action DoOnMouseDrag = DoNothing;
    public Action DoOnGUI = DoNothing;
    public Func<IEnumerator> ExitState = DoNothingCoroutine;
    private Enum _currentState;

    public Enum currentState {
      get {
        return _currentState;
      }
      set {
        _currentState = value;
        ConfigureCurrentState();
      }
    }

    public void ConfigureCurrentState() {
      state = _currentState.ToString();
      if (ExitState != null) {
        StartCoroutine(ExitState());
      }

      //Configure Delegate Methods
      DoUpdate = ConfigureDelegate<Action>("Update", DoNothing);
      DoOnGUI = ConfigureDelegate<Action>("OnGUI", DoNothing);
      DoLateUpdate = ConfigureDelegate<Action>("LateUpdate", DoNothing);
      DoFixedUpdate = ConfigureDelegate<Action>("FixedUpdate", DoNothing);
      DoOnMouseUp = ConfigureDelegate<Action>("OnMouseUp", DoNothing);
      DoOnMouseDown = ConfigureDelegate<Action>("OnMouseDown", DoNothing);
      DoOnMouseEnter = ConfigureDelegate<Action>("OnMouseEnter", DoNothing);
      DoOnMouseExit = ConfigureDelegate<Action>("OnMouseExit", DoNothing);
      DoOnMouseDrag = ConfigureDelegate<Action>("OnMouseDrag", DoNothing);
      DoOnMouseOver = ConfigureDelegate<Action>("OnMouseOver", DoNothing);
      DoOnTriggerEnter = ConfigureDelegate<Action<Collider>>("OnTriggerEnter", DoNothingCollider);
      DoOnTriggerExit = ConfigureDelegate<Action<Collider>>("OnTriggerExir", DoNothingCollider);
      DoOnTriggerStay = ConfigureDelegate<Action<Collider>>("OnTriggerEnter", DoNothingCollider);
      DoOnCollisionEnter = ConfigureDelegate<Action<Collision>>("OnCollisionEnter", DoNothingCollision);
      DoOnCollisionExit = ConfigureDelegate<Action<Collision>>("OnCollisionExit", DoNothingCollision);
      DoOnCollisionStay = ConfigureDelegate<Action<Collision>>("OnCollisionStay", DoNothingCollision);
      Func<IEnumerator> enterState = ConfigureDelegate<Func<IEnumerator>>("EnterState", DoNothingCoroutine);
      ExitState = ConfigureDelegate<Func<IEnumerator>>("ExitState", DoNothingCoroutine);

      //Optimisation, turn off GUI if we don't
      //have an OnGUI method
      EnableGUI();

      //Start the current state
      StartCoroutine(enterState());
    }

    //Define a generic method that returns a delegate
    //Note the where clause - we need to ensure that the
    //type passed in is a class and not a value type or our
    //cast (As T) will not work
    T ConfigureDelegate<T>(string methodRoot, T Default) where T : class {
      //Find a method called CURRENTSTATE_METHODROOT
      //The method can be either public or private
      var mtd = GetType().GetMethod(_currentState.ToString() + "_" + methodRoot, System.Reflection.BindingFlags.Instance
          | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod);
      //If we found a method
      if (mtd != null) {
        //Create a delegate of the type that this
        //generic instance needs and cast it
        return Delegate.CreateDelegate(typeof(T), this, mtd) as T;
      } else {
        //If we didn't find a method return the default
        return Default;
      }
    }

    //Call the delegates
    void Update() {
      Always_BeforeUpdate();
      DoUpdate();
      Always_AfterUpdate();
    }

    void LateUpdate() {
      Always_LateUpdate();
      DoLateUpdate();
    }

    void FixedUpdate() {
      Always_FixedUpdate();
      DoFixedUpdate();
    }

    void OnGUI() {
      DoOnGUI();
    }

    void OnMouseUp() {
      DoOnMouseUp();
    }

    void OnMouseDown() {
      DoOnMouseDown();
    }

    void OnMouseOver() {
      DoOnMouseOver();
    }

    void OnMouseExit() {
      DoOnMouseExit();
    }

    void OnMouseDrag() {
      DoOnMouseDrag();
    }

    void OnCollisionEnter(Collision other) {
      Debug.Log("smoo");
      DoOnCollisionEnter(other);
    }

    void OnCollisionExit(Collision other) {
      DoOnCollisionExit(other);
    }

    void OnCollisionStay(Collision other) {
      DoOnCollisionStay(other);
    }

    void OnTriggerEnter(Collider other) {
      DoOnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other) {
      DoOnTriggerStay(other);
    }

    void OnTriggerExit(Collider other) {
      DoOnTriggerExit(other);
    }

    //Helper Methods.
    private void EnableGUI() {
      useGUILayout = DoOnGUI != DoNothing;
    }
  }
}
