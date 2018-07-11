using UnityEngine;

public class CartPoleAgent : Agent
{
    /// <summary>
    /// カートの GameObject を Unity Editor で指定してください。
    /// </summary>
    public Rigidbody cartRigidbody;

    /// <summary>
    /// ポールの GameObject を Unity Editor で指定してください。
    /// </summary>
    public Rigidbody poleRigidbody;

    /// <summary>
    /// 観測した状態を Unity Editor で確認してください。
    /// </summary>
    public Vector3 observations;

    /// <summary>
    /// Brain で決定した行動を Unity Editor で確認してください。
    /// </summary>
    public float action;

    /// <summary>
    /// カートの移動速度係数。
    /// </summary>
    private float speed = 10f;

    /// <summary>
    /// Brain Type が Player の場合に、キーボードで Agent を操作します。
    /// </summary>
    private void FixedUpdate()
    {
        if (brain.brainType == BrainType.Player)
        {
            action = Input.GetAxis("Horizontal");
            action = Mathf.Clamp(action, -1f, 1f);

            var rigidbody = cartRigidbody;
            var position = rigidbody.position + rigidbody.transform.TransformDirection(Vector3.right * action * Time.fixedDeltaTime * speed);
            rigidbody.MovePosition(position);
        }
    }

    /// <summary>
    /// Agent の初期化を行います。
    /// ゲームを起動したときに一度だけ呼ばれます。
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    /// <summary>
    /// Agent の状態を観測して TensorFlow に受け渡します。
    /// 毎フレーム呼ばれます。
    /// </summary>
    public override void CollectObservations()
    {
        base.CollectObservations();

        Debug.Assert(brain.brainParameters.vectorObservationSpaceType == SpaceType.continuous);
        Debug.Assert(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous);

        // [0] カートの座標
        observations[0] = cartRigidbody.transform.localPosition.x;

        // [1] ポールの角度
        observations[1] = poleRigidbody.transform.localRotation.z;

        // [2] ポールの角速度
        observations[2] = poleRigidbody.angularVelocity.z;

        AddVectorObs(observations);
    }

    /// <summary>
    /// TensorFlow から受け取った行動ベクトルを Agent に実際に作用させます。
    /// 毎フレーム呼ばれます。
    /// </summary>
    /// <param name="vectorAction">TensorFlow が出力した行動ベクトル。</param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);

        // [0] カートの移動量
        action = vectorAction[0];
        action = Mathf.Clamp(action, -1f, 1f);

        // カートを動かす。
        var position = cartRigidbody.position + cartRigidbody.transform.TransformDirection(Vector3.right * action * Time.fixedDeltaTime * speed);
        cartRigidbody.MovePosition(position);

        // 1 エピソードが終了するまで、正の報酬 0.01 を加え続ける。
        SetReward(0.01f);

        // 終了条件を満たした場合、負の報酬 -1 を与え、エピソードを終了する。
        bool condition = false;
        condition |= (60f < Vector3.Angle(poleRigidbody.transform.up, Vector3.up)); // ポールが傾きすぎた。
        condition |= Mathf.Abs(cartRigidbody.transform.localPosition.x) > 4f;       // カートがステージの端に行った。
        if (condition)
        {
            Done();
            SetReward(-1f);
        }
    }

    /// <summary>
    /// Agent を初期状態に戻します。
    /// エピソードの開始時に呼ばれます。
    /// </summary>
    public override void AgentReset()
    {
        base.AgentReset();

        // カートを初期状態に戻します。
        {
            var transform = cartRigidbody.transform;
            var rigidbody = cartRigidbody;
            transform.localPosition = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        // ポールを初期状態に戻します。
        {
            var transform = poleRigidbody.transform;
            var rigidbody = poleRigidbody;
            transform.localPosition = new Vector3(0f, 2f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
