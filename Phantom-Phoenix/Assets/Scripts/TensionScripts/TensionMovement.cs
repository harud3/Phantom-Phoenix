using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class TensionMovement : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private GameObject HintMessage;
    public void OnPointerDown(PointerEventData eventData)
    {
        TensionController tensionController = GetComponent<TensionController>();

        if (!tensionController.model.isPlayer) { return; }
        //�����̃^�[���Ɏ����̃e���V�����𑀍�ł���@�܂��A�^�[�����Ƀe���V�������g�p���Ă���ƃe���V�����̎g�p�͂ł��Ȃ�
        //�������A�e���V����3�̓e���V�����X�y�����g�p�ł����ԂȂ̂ő���\
        if (GameManager.instance.isPlayerTurn == tensionController.model.isPlayer)
        {
            if (tensionController.model.tension == 3)
            {
                switch (tensionController.model.tensionID)
                {
                    case 1: //elf
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //�����ɓ����Ă��Ă��鎞�_��cc��hc�̂ǂ��炩�͑ΏۂƂȂ��Ă���
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 2: //witch
                        if (IsExistTarget(2))
                        {
                            StartCoroutine(waitPlayerClick(tensionController));
                        }
                        break;
                    case 3: //king
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //�����ɓ����Ă��Ă��鎞�_��cc��hc�̂ǂ��炩�͑ΏۂƂȂ��Ă���
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 4: //demon
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //�����ɓ����Ă��Ă��鎞�_��cc��hc�̂ǂ��炩�͑ΏۂƂȂ��Ă���
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 5: //knight
                        if (IsExistTarget(5))
                        {
                            StartCoroutine(waitPlayerClick(tensionController));
                        }
                        break;
                }

            }
            else if (!tensionController.model.isTensionUsedThisTurn)
            {
                tensionController.UseTensionCard();
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendUseTensionCard();
                }
            }

        }
    }
    /// <summary>
    ///�@���͑҂��@���͂�����΁A���ʂ̔���Ɉڂ�    �������ꂪ��Ԓx���Ǝv���܂�
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    IEnumerator waitPlayerClick(TensionController tc)
    {
        HintMessage.SetActive(true); //�q���g�̕\��

        yield return null;
        //���͑҂�
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        FieldManager.instance.SetSelectablePanel(Enumerable.Range(1, 12).ToArray(), false); //�t�B�[���h�̑I���\�p�l�����\���ɂ���
        FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), false);
        HintMessage.SetActive(false);

        //���R�s�y�@���[���h���W���Ƀn�}���Ēɂ��ڂ�����
        GameObject clickedGameObject = null;

        //RaycastAll�̈����iPointerEventData�j�쐬
        PointerEventData pointData = new PointerEventData(EventSystem.current);

        //RaycastAll�̌��ʊi�[�pList
        List<RaycastResult> RayResult = new List<RaycastResult>();

        //PointerEventData�Ƀ}�E�X�̈ʒu���Z�b�g
        pointData.position = Input.mousePosition;
        //RayCast�i�X�N���[�����W�j
        EventSystem.current.RaycastAll(pointData, RayResult);
        //���R�s�y

        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card" || i.gameObject.tag == "Hero").FirstOrDefault().gameObject; //tag�Ŕ��f���邱�Ƃɂ����@���j�b�g��q�[���[���d�Ȃ��Ă邱�Ƃ͂Ȃ�
        if (TargetCheck(clickedGameObject, tc) is var x && x.passed)
        {
            if (x.cctarget != null)
            {
                tc.UseTensionSpell(x.cctarget);

            }
            else if (x.hctarget != null)
            {
                tc.UseTensionSpell(x.hctarget);
            }
            if (GameDataManager.instance.isOnlineBattle) //�����ɓ����Ă��Ă��鎞�_��cc��hc�̂ǂ��炩�͑ΏۂƂȂ��Ă���
            {
                GameManager.instance.SendUseTensionSpell(x.targetFieldIDByReceiver);
            }
        }
    }
    /// <summary>
    /// �ΏۂƂȂ肤���₪���邩�m�F
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    private bool IsExistTarget(int tensionID)
    {
        switch (tensionID)
        {
            case 2: //witch
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������

                    }
                    FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), true);
                    return true;
                }
            case 5: //knight
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true);
                        return true;
                    }
                    break;
                }
        }
        return false;
    }
    /// <summary>
    /// �Ώۂ̊m�F targetsByReceiver�͑ΐ푊��ɑ��M�������
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="clickGameObject"></param>
    /// <returns></returns>
    private (bool passed, HeroController hctarget, CardController cctarget, int targetFieldIDByReceiver) TargetCheck(GameObject clickGameObject, TensionController tc)
    {

        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //��ꂽ�炢���ł���
        clickGameObject?.TryGetComponent<CardController>(out c); //��ꂽ�炢���ł���

        switch (tc.model.tensionID)
        {
            case 2:
                {
                    if (c != null)
                    {
                        return (true, hc, c, FieldManager.instance.ChangeFieldID(c.model.thisFieldID));
                    }
                    if (hc != null)
                    {
                        return (true, hc, c, hc.model.isPlayer ? 14 : 13); //13�͖����q�[���[ 14�͓G�q�[���[�Ƃ��� ��M�҂��猩���ΏۂȂ̂ŁA�����œG�����̔ԍ������ւ��Ă���
                    }
                    return (false, null, null, 0);
                }
            case 5:
                {
                    if (c != null && c.model.isPlayerCard != tc.model.isPlayer)
                    {
                        return (true, hc, c, FieldManager.instance.ChangeFieldID(c.model.thisFieldID));
                    }
                    return (false, null, null, 0);
                }
            default:
                {
                    return (false, null, null, 0);
                }
        }
    }
}
