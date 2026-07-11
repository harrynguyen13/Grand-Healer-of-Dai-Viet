using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcVillageRole
{
    DanOng,
    LaoOng,
    LaoBa,
    BeTrai,
    BeGai,
    PhuNu
}

[RequireComponent(typeof(NpcAIController))]
public class NpcConversationController : MonoBehaviour
{
    [Header("Bật / tắt hội thoại")]
    [SerializeField] private bool enableConversation = true;

    [Header("Vai trò trong làng")]
    [SerializeField] private NpcVillageRole villageRole = NpcVillageRole.DanOng;

    [Header("Cấu hình hội thoại")]
    [SerializeField] private float talkDurationPerLine = 1.8f;
    [SerializeField] private float talkCooldown = 8f;
    [SerializeField] private float separateMoveTime = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float talkChance = 0.75f;

    [Header("Bong bóng thoại")]
    [SerializeField] private NpcSpeechBubble speechBubble;

    private NpcAIController movement;
    private float nextTalkTime;
    private bool isTalking;
    private Coroutine conversationCoroutine;

    private struct ConversationTurn
    {
        public NpcConversationController speaker;
        public string line;

        public ConversationTurn(NpcConversationController speaker, string line)
        {
            this.speaker = speaker;
            this.line = line;
        }
    }

    private void Awake()
    {
        movement = GetComponent<NpcAIController>();

        if (speechBubble == null)
            speechBubble = GetComponent<NpcSpeechBubble>();

        HideBubble();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTalkFromCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryTalkFromCollision(collision);
    }

    private void TryTalkFromCollision(Collision2D collision)
    {
        if (!enableConversation)
            return;

        if (collision == null)
            return;

        if (!collision.gameObject.CompareTag("NPC"))
            return;

        NpcConversationController otherNpc =
            collision.gameObject.GetComponent<NpcConversationController>();

        if (otherNpc == null)
            otherNpc = collision.gameObject.GetComponentInParent<NpcConversationController>();

        if (otherNpc == null)
            return;

        TryStartConversation(otherNpc);
    }

    private void TryStartConversation(NpcConversationController otherNpc)
    {
        if (!enableConversation)
            return;

        if (otherNpc == null || !otherNpc.enableConversation)
            return;

        if (otherNpc == this)
            return;

        if (isTalking || otherNpc.isTalking)
            return;

        if (movement == null || otherNpc.movement == null)
            return;

        if (movement.IsBusy || otherNpc.movement.IsBusy)
            return;

        if (Time.time < nextTalkTime || Time.time < otherNpc.nextTalkTime)
            return;

        if (Random.value > talkChance)
        {
            movement.BounceAway();
            return;
        }

        if (conversationCoroutine != null)
            StopCoroutine(conversationCoroutine);

        conversationCoroutine = StartCoroutine(ConversationRoutine(otherNpc));
    }

    private IEnumerator ConversationRoutine(NpcConversationController otherNpc)
    {
        if (otherNpc == null)
            yield break;

        isTalking = true;
        otherNpc.isTalking = true;

        nextTalkTime = Time.time + talkCooldown;
        otherNpc.nextTalkTime = Time.time + otherNpc.talkCooldown;

        movement.SetBusy(true);
        otherNpc.movement.SetBusy(true);

        movement.ForceStopMovement();
        otherNpc.movement.ForceStopMovement();

        movement.FaceTarget(otherNpc.transform.position);
        otherNpc.movement.FaceTarget(transform.position);

        HideBubble();
        otherNpc.HideBubble();

        List<ConversationTurn> turns = BuildVillageConversation(otherNpc);

        for (int i = 0; i < turns.Count; i++)
        {
            HideBubble();
            otherNpc.HideBubble();

            if (turns[i].speaker != null)
                turns[i].speaker.ShowBubble(turns[i].line);

            yield return new WaitForSeconds(talkDurationPerLine);
        }

        HideBubble();
        otherNpc.HideBubble();

        movement.SetBusy(false);
        otherNpc.movement.SetBusy(false);

        SeparateFrom(otherNpc);

        isTalking = false;
        otherNpc.isTalking = false;

        Debug.Log(gameObject.name + " đã trò chuyện với " + otherNpc.gameObject.name);
    }

    private List<ConversationTurn> BuildVillageConversation(NpcConversationController otherNpc)
    {
        List<ConversationTurn> turns = new List<ConversationTurn>();

        if (otherNpc == null)
            return turns;

        // Ông / bà lão gặp trẻ em
        if (IsElder(villageRole) && otherNpc.IsChild())
        {
            AddRandomConversation(
                turns,
                this,
                otherNpc,
                new string[]
                {
                    "Cháu đang làm gì đấy?",
                    "Đi đâu mà vội thế cháu?",
                    "Cháu lại chạy chơi ngoài đường à?",
                    "Trời nắng rồi, cháu không đội nón sao?"
                },
                new string[]
                {
                    "Cháu đang đi chơi ạ.",
                    "Cháu ra gốc đa chơi một lát ạ.",
                    "Cháu đi tìm bạn ạ.",
                    "Cháu chơi một lát rồi về ạ."
                }
            );

            return turns;
        }

        if (IsChild() && otherNpc.IsElder())
        {
            AddRandomConversation(
                turns,
                otherNpc,
                this,
                new string[]
                {
                    "Cháu đang làm gì đấy?",
                    "Đi đâu mà vội thế cháu?",
                    "Cháu lại chạy chơi ngoài đường à?",
                    "Trời nắng rồi, cháu không đội nón sao?"
                },
                new string[]
                {
                    "Cháu đang đi chơi ạ.",
                    "Cháu ra gốc đa chơi một lát ạ.",
                    "Cháu đi tìm bạn ạ.",
                    "Cháu chơi một lát rồi về ạ."
                }
            );

            return turns;
        }

        // Trẻ em gặp trẻ em
        if (IsChild() && otherNpc.IsChild())
        {
            AddRandomConversation(
                turns,
                this,
                otherNpc,
                new string[]
                {
                    "Ra gốc đa chơi không?",
                    "Đi bắt chuồn chuồn không?",
                    "Cậu có thấy con mèo nhà bác Ba không?",
                    "Ra bờ ruộng chơi một lát không?"
                },
                new string[]
                {
                    "Đi, nhưng đừng chạy xa quá.",
                    "Không, mẹ dặn tớ không được đi xa.",
                    "Không thấy, chắc nó chạy ra vườn rồi.",
                    "Tớ phải về trước bữa cơm."
                }
            );

            return turns;
        }

        // Người lớn gặp trẻ em
        if (IsAdult() && otherNpc.IsChild())
        {
            AddRandomConversation(
                turns,
                this,
                otherNpc,
                new string[]
                {
                    "Cháu nhớ về nhà sớm nhé.",
                    "Đừng chạy gần bờ ao đấy.",
                    "Trời nắng rồi, đừng chơi lâu quá.",
                    "Chơi thì chơi, đừng làm bẩn áo nhé."
                },
                new string[]
                {
                    "Vâng ạ.",
                    "Cháu biết rồi ạ.",
                    "Cháu chơi một lát rồi về ạ.",
                    "Dạ, cháu nhớ rồi."
                }
            );

            return turns;
        }

        if (IsChild() && otherNpc.IsAdult())
        {
            AddRandomConversation(
                turns,
                otherNpc,
                this,
                new string[]
                {
                    "Cháu nhớ về nhà sớm nhé.",
                    "Đừng chạy gần bờ ao đấy.",
                    "Trời nắng rồi, đừng chơi lâu quá.",
                    "Chơi thì chơi, đừng làm bẩn áo nhé."
                },
                new string[]
                {
                    "Vâng ạ.",
                    "Cháu biết rồi ạ.",
                    "Cháu chơi một lát rồi về ạ.",
                    "Dạ, cháu nhớ rồi."
                }
            );

            return turns;
        }

        // Bà lão / ông lão gặp người lớn
        if (IsElder(villageRole) && otherNpc.IsAdult())
        {
            AddRandomConversation(
                turns,
                this,
                otherNpc,
                new string[]
                {
                    "Dạo này trong làng có nhiều người ho quá.",
                    "Trời trở gió, người già dễ đau nhức lắm.",
                    "Nghe nói y quán hôm nay đông người.",
                    "Mùa này phải giữ ấm mới được."
                },
                new string[]
                {
                    "Vâng, chắc phải ghé y quán hỏi thăm.",
                    "Bà nhớ giữ sức khỏe nhé.",
                    "Đúng là thời tiết thất thường quá.",
                    "Cháu cũng nghe nhiều người nói vậy."
                }
            );

            return turns;
        }

        if (IsAdult() && otherNpc.IsElder())
        {
            AddRandomConversation(
                turns,
                otherNpc,
                this,
                new string[]
                {
                    "Dạo này trong làng có nhiều người ho quá.",
                    "Trời trở gió, người già dễ đau nhức lắm.",
                    "Nghe nói y quán hôm nay đông người.",
                    "Mùa này phải giữ ấm mới được."
                },
                new string[]
                {
                    "Vâng, chắc phải ghé y quán hỏi thăm.",
                    "Bà nhớ giữ sức khỏe nhé.",
                    "Đúng là thời tiết thất thường quá.",
                    "Cháu cũng nghe nhiều người nói vậy."
                }
            );

            return turns;
        }

        // Người lớn gặp người lớn
        AddRandomConversation(
            turns,
            this,
            otherNpc,
            new string[]
            {
                "Dạo này trời trở gió quá.",
                "Nghe nói y quán hôm nay đông người.",
                "Nhà bên kia hình như có người bị ho.",
                "Ruộng ngoài kia năm nay có vẻ tốt.",
                "Mấy hôm nay đường làng đông hơn hẳn."
            },
            new string[]
            {
                "Ừ, phải giữ ấm mới được.",
                "Chắc lát nữa nên ghé qua y quán.",
                "Mong trong làng được bình an.",
                "Cầu cho mùa này thuận lợi.",
                "Chắc sắp vào vụ mới rồi."
            }
        );

        return turns;
    }

    private void AddRandomConversation(
        List<ConversationTurn> turns,
        NpcConversationController firstSpeaker,
        NpcConversationController secondSpeaker,
        string[] questions,
        string[] answers
    )
    {
        if (turns == null)
            return;

        string question = GetRandomFromArray(questions);
        string answer = GetRandomFromArray(answers);

        turns.Add(new ConversationTurn(firstSpeaker, question));
        turns.Add(new ConversationTurn(secondSpeaker, answer));
    }

    private string GetRandomFromArray(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return "...";

        int index = Random.Range(0, lines.Length);
        return lines[index];
    }

    private bool IsChild()
    {
        return IsChild(villageRole);
    }

    private bool IsChild(NpcVillageRole role)
    {
        return role == NpcVillageRole.BeTrai || role == NpcVillageRole.BeGai;
    }

    private bool IsElder()
    {
        return IsElder(villageRole);
    }

    private bool IsElder(NpcVillageRole role)
    {
        return role == NpcVillageRole.LaoOng || role == NpcVillageRole.LaoBa;
    }

    private bool IsAdult()
    {
        return IsAdult(villageRole);
    }

    private bool IsAdult(NpcVillageRole role)
    {
        return role == NpcVillageRole.DanOng
            || role == NpcVillageRole.PhuNu
            || IsElder(role);
    }

    private void ShowBubble(string line)
    {
        if (speechBubble != null)
            speechBubble.Show(line);
    }

    private void HideBubble()
    {
        if (speechBubble != null)
            speechBubble.Hide();
    }

    private void SeparateFrom(NpcConversationController otherNpc)
    {
        if (otherNpc == null || movement == null || otherNpc.movement == null)
            return;

        Vector2 awayDirection = transform.position - otherNpc.transform.position;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = Vector2.right;

        awayDirection.Normalize();

        movement.MoveDirectionForSeconds(awayDirection, separateMoveTime);
        otherNpc.movement.MoveDirectionForSeconds(-awayDirection, otherNpc.separateMoveTime);
    }
}