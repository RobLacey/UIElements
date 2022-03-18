using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class AnswerMachine : AudioPlayer
{
    [Required("Must Assign a Text Element")]
    [SerializeField] private Text _answerText;

    protected override void Awake()
    {
        base.Awake();
        _answerText.enabled = false;
    }

    public override void Play(bool doPlay)
    {
        _answerText.enabled = true;
        base.Play(doPlay);
    }

}
