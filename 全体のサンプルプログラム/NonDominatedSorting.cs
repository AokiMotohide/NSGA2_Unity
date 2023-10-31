//�u��x�z�\�[�g�v�E�u���G�x�̌v�Z�v���s���N���X

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class NonDominatedSorting
{

    public static List<int> Sort(List<Sample_Solution> solutions) //�e�v�f�̒l���擾
    {
        int N = solutions.Count;  //�����̐����J�E���g�A
        List<int> frontLevels = new List<int>(new int[N]); //�x�z�\�[�g���x��
        List<int>[] dominatedBy = new List<int>[N]; //��i���x�z���邷�ׂẲ��̃C���f�b�N�X��ۑ����܂��B
        int[] numDominated = new int[N]; //��i���x�z���鑼�̉��̐�

        //�����̐����ƂɃ��[�v
        for (int i = 0; i < N; i++)
        {
            dominatedBy[i] = new List<int>();
            //�v�f���ƂɂQ���ڂ̃��[�v
            for (int j = 0; j < N; j++)
            {
                if (i != j)
                {
                    // i��j�Ɏx�z����邩�ǂ������m�F���܂��B
                    if (Dominates(solutions[j], solutions[i])) //����i��j���x�z���Ă�����
                    {
                        dominatedBy[i].Add(j); //��i���x�z���邷�ׂẲ��̃C���f�b�N�X��ۑ�����
                    }
                    else if (Dominates(solutions[i], solutions[j])) //����i��j�Ɏx�z����Ă�����
                    {
                        numDominated[i]++; //��i���x�z����鑼�̉��̐���ۑ����܂��B
                    }
                }
            }

            //��x�z�����[���Ȃ烉���N�P�i,���������N���������l���������A�����N���Ⴂ�F�l���傫���j�����N�P���ō�
            if (numDominated[i] == 0)
            {
                frontLevels[i] = 1;
            }
        }

        for (int i = 0; i < N; i++)
        {
            foreach (int j in dominatedBy[i]) //��i���x�z���邷�ׂẲ�j�ɑ΂��ă��[�v����
            {
                numDominated[j]--; //��j���x�z����Ă��鐔��1���������܂��B����́A��i����j���x�z���邱�Ƃ��m�F���ꂽ���߂ł��B
                if (numDominated[j] == 0)
                {
                    frontLevels[j] = frontLevels[i] + 1;
                }
            }
        }

        return frontLevels;
    }


    ///���G�x�̌v�Z
    public static List<double> CalculateCrowdingDistance(List<Sample_Solution> front)
    {
        int size = front.Count; //�t�����g�̉��̐�

        // �e���̍��G�x��ۑ����邽�߂̃��X�g��������
        List<double> crowdingDistance = new List<double>(new double[size]);

        // �t�����g�ɉ����Ȃ��ꍇ
        if (size == 0)
            return crowdingDistance;

        // �t�����g��1�̉��݂̂�����ꍇ���G�x�͖�����
        if (size == 1)
        {
            crowdingDistance[0] = double.PositiveInfinity;  //���̖�����
            return crowdingDistance;
        }

        // �t�����g��2�̉�������ꍇ�A���G�x�͖�����
        if (size == 2)
        {
            crowdingDistance[0] = double.PositiveInfinity;
            crowdingDistance[1] = double.PositiveInfinity;
            return crowdingDistance;
        }


        //�ړI�֐����Ƃɉ����\�[�g
        List<int>[] sortedIndicesByObjective = new List<int>[3];  // �e�ړI�֐����Ƃɉ����\�[�g���邽�߂̃C���f�b�N�X��ۑ�
        for (int i = 0; i < 3; i++)
        {
            sortedIndicesByObjective[i] = SortByObjective(front, i); //�ړI�֐����Ƃɉ����\�[�g
                                                                     //�e�ړI�֐��ɂ����ă\�[�g���ꂽ���̍ŏ��ƍŌ�i���E���j�̍��G�x�𖳌���ɐݒ�
            crowdingDistance[sortedIndicesByObjective[i][0]] = double.PositiveInfinity;
            crowdingDistance[sortedIndicesByObjective[i][size - 1]] = double.PositiveInfinity;
        }


        // ���Ԃ̉��̍��G�x���v�Z�Asize�͔z��̃T�C�Y
        for (int i = 1; i < size - 1; i++)
        {
            double distance = 0.0;

            //�ړI�֐����Ƃ̍��G�x�̌v�Z
            for (int j = 0; j < 3; j++)
            {
                double objectiveDiff = GetObjectiveValue(front[sortedIndicesByObjective[j][i + 1]], j) -
                                        GetObjectiveValue(front[sortedIndicesByObjective[j][i - 1]], j);
                distance += objectiveDiff; //�ړI�֐����Ƃ̋���
            }
            crowdingDistance[sortedIndicesByObjective[0][i]] = distance; //���G�x
        }

        return crowdingDistance;
    }




    // �w�肳�ꂽ�ړI�֐��Ɋ�Â��ăt�����g�̉����\�[�g����֐�
    private static List<int> SortByObjective(List<Sample_Solution> front, int objectiveIndex)
    {
        List<int> indices = new List<int>(front.Count);
        for (int i = 0; i < front.Count; i++)
        {
            indices.Add(i);
        }

        //�����_�����g�����\�[�g,(a, b) => a.CompareTo(b)�ŕ��בւ��邱�Ƃ��ł���
        indices.Sort((x, y) => GetObjectiveValue(front[x], objectiveIndex).CompareTo(GetObjectiveValue(front[y], objectiveIndex)));

        return indices;
    }

    // ���ƖړI�֐��̃C���f�b�N�X���󂯎��A���̖ړI�֐��̒l��Ԃ��֐�
    private static double GetObjectiveValue(Sample_Solution solution, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= solution.objectives.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid objective index.");
        }

        return solution.objectives[objectiveIndex];
    }

    private static bool Dominates(Sample_Solution a, Sample_Solution b)
    {
        bool betterInAnyObjective = false;

        // �S�Ă̖ړI�֐��iobjectives�j�ɂ����Ĕ�x�z�̊֌W�𒲂ׂ�
        bool isBetterOrEqualInAllObjectives = true;
        for (int i = 0; i < a.objectives.Length; i++)
        {
            if (a.objectives[i] > b.objectives[i])
            {
                isBetterOrEqualInAllObjectives = false;
                break;
            }
        }

        if (isBetterOrEqualInAllObjectives)
        {
            for (int i = 0; i < a.objectives.Length; i++)
            {
                if (a.objectives[i] < b.objectives[i])
                {
                    betterInAnyObjective = true;
                    break;
                }
            }
        }

        return betterInAnyObjective;
    }

}
