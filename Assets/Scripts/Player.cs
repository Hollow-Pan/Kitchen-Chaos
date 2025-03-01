using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent{    

    public static Player Instance { get; private set; }

    public event EventHandler OnObjectPickup;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs{
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform KitchenObjectHoldPoint;

    private bool isWalking;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;
    private Vector3 lastInteractDir;

    private void Awake(){
        if (Instance != null){
            Debug.LogError("There is more than one Player instance");
        }
        Instance = this;
    }

    private void Start(){
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e){
        if (KitchenGameManager.Instance.IsGamePlaying()){
            if (selectedCounter != null){
                selectedCounter.InteractAlternate(this);
            }
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e){
        if (KitchenGameManager.Instance.IsGamePlaying()){
            if (selectedCounter != null){
                selectedCounter.Interact(this);
            }
        }
    }
    private void Update(){
        HandleMovements();
        HandleInteractions();
    }
    public bool IsWalking(){
        return isWalking;
    }

    private void HandleInteractions(){
        Vector2 inputVector = gameInput.GetNormalisedMovementVector();
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (movDir != Vector3.zero){
            lastInteractDir = movDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)){
                //has ClearCounter
                if (baseCounter != selectedCounter){
                    SetSelectedCounter(baseCounter);                    
                }
            }
            else{
                SetSelectedCounter(null);
            }
        }
        else{
            SetSelectedCounter(null);
        }
    }

    private void HandleMovements(){
        Vector2 inputVector = gameInput.GetNormalisedMovementVector();
        Vector3 movDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDir, moveDistance);

        if (!canMove){

            //Attempt X movement
            Vector3 movDirX = new Vector3(movDir.x, 0, 0).normalized;
            canMove = movDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirX, moveDistance);

            if (canMove){
                movDir = movDirX;
            }
            else{
                //Cannot move on X
                //Attempt Z movement
                Vector3 movDirZ = new Vector3(0, 0, movDir.z).normalized;
                canMove = movDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirZ, moveDistance);

                if (canMove){
                    movDir = movDirZ;
                }
                else{
                    //Cannot move at all
                }
            }
        }

        if (canMove){
            transform.position += movDir * moveSpeed * Time.deltaTime;
        }

        isWalking = movDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movDir, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter){
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this,new OnSelectedCounterChangedEventArgs{
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform(){
        return KitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null){
            OnObjectPickup?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void ClearKitchenObject(){
        kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }

}
