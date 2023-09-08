using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerKit
{
    /// <summary>
    /// 2D角色控制器
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField] private float mNormalSpeed; //正常速度
        [SerializeField] private float mDashSpeed; //冲刺速度
        [SerializeField] private float mCrouchSpeed; //蹲下时速度
        [SerializeField] private float mJumpForce; //跳跃时的力

        [SerializeField] private float mMovementSmoothing = .04f; //移动平滑度

        [SerializeField] private Transform mGroundCheck; //地面检测圆心
        [SerializeField] private float mGroundCheckRadius = .5f; //地面检测半径
        [SerializeField] private Transform mCeilingCheck; //天花板检测圆心
        [SerializeField] private float mCeilingCheckRadius = .5f; //天花板检测半径

        [SerializeField] private bool mAirControl; //跳跃时是否能操控
        [SerializeField] private LayerMask mGroundLayer; //地面的layer
        [SerializeField] private Collider2D mCrouchDisableCollider; //蹲下时会取消的碰撞体

        private Rigidbody2D _mRb;
        public static bool MGrounded = true; //玩家是否在地面上
        private bool _mWasCrouch; //玩家是否蹲下
        private Vector3 _mVelocity;

        [Header("行动触发事件")] [Space(10)] public UnityEvent onLandEvent; //着陆时触发的事件

        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        public BoolEvent onCrouchEvent; //下蹲时触发的事件

        private void Awake()
        {
            _mRb = GetComponent<Rigidbody2D>();

            if (onLandEvent == null)
                onLandEvent = new UnityEvent();
            if (onCrouchEvent == null)
                onCrouchEvent = new BoolEvent();

        }

        /// <summary>
        /// 地面检测
        /// </summary>
        private void FixedUpdate()
        {
            //使用临时变量wasGrounded来记录地面检测的变化情况，正常移动时wasGrounded总为true,
            //跳跃并着陆时会触发一次变化而呼叫对应事件
            //若想让玩家在开始时触发一次落地，将m_Grounded的初始值置为false即可
            bool wasGrounded = MGrounded;

            //每一个检测到的碰撞体都会触发一次落地事件
            Collider2D[] colliders =
                Physics2D.OverlapCircleAll(mGroundCheck.position, mGroundCheckRadius, mGroundLayer);
            
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    MGrounded = true;
                    if (!wasGrounded)
                        onLandEvent.Invoke();
                    
                    return;
                }
                else
                    MGrounded = false;
            }
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="crouch"></param>
        /// <param name="jump"></param>
        public void Move(Vector2 dir, bool crouch, bool jump)
        {
            float currentSpeed = 0f;
            
            //玩家取消下蹲时需要检测周围是否有阻止起身的物体，如果有，保持下蹲
            if (!crouch)
            {
                if (Physics2D.OverlapCircle(mCeilingCheck.position, mCeilingCheckRadius, mGroundLayer))
                    crouch = true;
            }

            //检查玩家是否处于可操控状态
            if (MGrounded || mAirControl)
            {
                //下蹲的判断
                if (crouch)
                {
                    if (!_mWasCrouch)
                    {
                        _mWasCrouch = true;
                        onCrouchEvent.Invoke(true);
                    }

                    //进行减速
                    currentSpeed = mCrouchSpeed;

                    //取消对应碰撞体
                    if (mCrouchDisableCollider != null)
                        mCrouchDisableCollider.enabled = false;
                }
            }
            else
            {
                if (mCrouchDisableCollider != null)
                    mCrouchDisableCollider.enabled = true;

                if (_mWasCrouch)
                {
                    _mWasCrouch = false;
                    onCrouchEvent.Invoke(false);
                }

                //恢复原速
                currentSpeed = mNormalSpeed;
            }

            //移动角色
            Vector3 targetVelocity = new Vector2(dir.x * currentSpeed * Time.deltaTime, _mRb.velocity.y);
            //增加平滑
            _mRb.velocity = Vector3.SmoothDamp(_mRb.velocity, targetVelocity, ref _mVelocity,
                mMovementSmoothing);
            //检测面向
            if (Mathf.Sign(transform.localScale.x) != Mathf.Sign(dir.x))
                Flip();

            //玩家按下跳跃
            if (MGrounded && jump)
            {
                //增加力
                MGrounded = false;
                _mRb.AddForce(new Vector2(0f, mJumpForce));
            }
        }

        /// <summary>
        /// 玩家转向
        /// </summary>
        private void Flip()
        {
            Vector3 theScale = transform.localScale * -1;
            transform.localScale = theScale;
        }
    }
}