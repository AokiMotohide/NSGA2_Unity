using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample_Solution
{


    //��D��\�[�g�ɂ����郉���N
    public int Rank { get; set; } 

    // numberOfVariables�Ɋ�Â��ĕϐ��̃T�C�Y�����������邽�߂̐V�����v���p�e�B�ƃR���X�g���N�^
    public float[] variables;
    public float[] objectives;

    public Sample_Solution(int numberOfVars)
    {
        variables = new float[numberOfVars];
        objectives = new float[3]; // 3�̖ړI�֐��������Ă���Ɖ��肵�Ă��܂�
    }

    // ���̒ǉ��̑����⃁�\�b�h���K�v�ȏꍇ�͂�����ɒǉ����Ă��������B
}