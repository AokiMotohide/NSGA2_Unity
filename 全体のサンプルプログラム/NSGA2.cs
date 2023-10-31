using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class NSGAII
{
    // �|�s�����[�V�����̐ݒ�l
    public static int populationSize;
    public static int generations;
    public static float mutationProbability;
    public static float crossoverProbability;
    public static int numberOfVariables;
    public static float lowerBound;
    public static float upperBound;

    //public static List<Sample_Solution> archivePopulation = new List<Sample_Solution>(); // �A�[�J�C�u��W�c��ێ����郊�X�g


    // �R���X�g���N�^�F�������Ɛݒ�
    public NSGAII(int _populationSize, int _generations, float _mutationProbability, float _crossoverProbability, int _numberOfVariables, float _lowerBound, float _upperBound)
    {
        populationSize = _populationSize;
        generations = _generations;
        mutationProbability = _mutationProbability;
        crossoverProbability = _crossoverProbability;
        numberOfVariables = _numberOfVariables;
        lowerBound = _lowerBound;
        upperBound = _upperBound;
    }

    // �i���̎�v�ȃ��[�`��
    public void Evolve()
    {
        List<Sample_Solution> population = InitializePopulation(); // �����̃|�s�����[�V�����𐶐�
        Evaluate(population); // �|�s�����[�V�����̕]��                        
        NSGA2_Run.archivePopulation = ExtractNewArchive(population); // �����̃A�[�J�C�u��W�c��������

        //�i���J�n
        for (int i = 0; i < generations; i++)
        {

            List<Sample_Solution> offspringPopulation = NextGeneration(population); //�I�t�X�v�����O�͎��̐���̎q��W�c
            List<Sample_Solution> combinedPopulation = NSGA2_Run.archivePopulation.Concat(offspringPopulation).ToList(); // �A�[�J�C�u��W�c�Ə���W�c������

            //�S��W�c���D��\�[�g�A���G�x�\�[�g�ɂ���đI�ʂ��A�A�[�J�C�u��W�c���쐬����
            List<Sample_Solution> newArchive = ExtractNewArchive(combinedPopulation);

            //�A�[�J�C�u��W�c�̊���
            NSGA2_Run.archivePopulation = newArchive;

            //�q��W�c�̐���
            population = NSGA2_Run.archivePopulation;



        }
    }

    // �����̃|�s�����[�V�����𐶐�
    public static List<Sample_Solution> InitializePopulation()
    {
        List<Sample_Solution>population2 = new List<Sample_Solution>();

        for (int i = 0; i < populationSize; i++)
        {
            Sample_Solution sol = new Sample_Solution(numberOfVariables);
            for (int j = 0; j < numberOfVariables; j++)
            {
                sol.variables[j] = UnityEngine.Random.Range(lowerBound, upperBound); // �ϐ��������_���ɏ�����
            }
            population2.Add(sol);
        }
        return population2;
    }

    // �|�s�����[�V�����̕]��
    public static void Evaluate(List<Sample_Solution> population)
    {
        foreach (Sample_Solution sol in population)
        {
            sol.objectives = DTLZ2(sol.variables); // DTLZ2�֐���p�����]��
        }
    }


    // DTLZ2�֐��̎���
    public static float[] DTLZ2(float[] x)
    {
        int M = 3;  // �ړI�֐��̐�
        int k = x.Length - M + 1;  // k parameter (usually k=10 for DTLZ2)

        float g = 0.0f;

        // g�̌v�Z (�Ō��k�ϐ��Ɋ�Â�)
        for (int i = 1; i < M; i++)
        {
            g += (x[i] - 0.5f) * (x[i] - 0.5f);
        }

        //for (int i = M; i < M + k; i++)
        //{
        //    g += (x[i - 1] - 0.5f) * (x[i - 1] - 0.5f);  // i-1 because array index starts from 0
        //}

        float[] objectives = new float[M];

        // �ړI�֐� f1
        objectives[0] = (1.0f + g) * Mathf.Cos(x[0] * Mathf.PI / 2) * Mathf.Cos(x[1] * Mathf.PI / 2);

        // �ړI�֐� f2
        objectives[1] = (1.0f + g) * Mathf.Cos(x[0] * Mathf.PI / 2) * Mathf.Sin(x[1] * Mathf.PI / 2);

        // �ړI�֐� f3
        objectives[2] = (1.0f + g) * Mathf.Sin(x[0] * Mathf.PI / 2);

        return objectives;


        /*
        int M = x.Length;
        float g = 0;

        for (int i = 1; i < M; i++)
        {
            g += (x[i] - 0.5f) * (x[i] - 0.5f);
        }

        float[] objectives = new float[3];
        objectives[0] = (1 + g) * Mathf.Cos(x[0] * Mathf.PI / 2);
        objectives[1] = (1 + g) * Mathf.Sin(x[0] * Mathf.PI / 2);
        objectives[2] = (1 + g);

        return objectives;
        */
    }


    // ���̐���̃|�s�����[�V�����𐶐�
    public static List<Sample_Solution> NextGeneration(List<Sample_Solution> population)
    {
        //���̐����Population��ێ�������
        List<Sample_Solution> offspringPopulationNext = new List<Sample_Solution>();

        //�l���̃T�C�Y�ɂȂ�܂�
        while (offspringPopulationNext.Count < populationSize)
        {
            // 1. �g�[�i�����g�I����p���ăy�A�̐e��I��
            Sample_Solution parent1 = TournamentSelect(population);
            Sample_Solution parent2 = TournamentSelect(population);

            // 2. ������p����2�̎q���𐶐�
            if (UnityEngine.Random.value < crossoverProbability) //�������邩���Ȃ���
            {
                List<Sample_Solution> children = Crossover(parent1, parent2);
                offspringPopulationNext.AddRange(children);
            }
            else
            {
                offspringPopulationNext.Add(parent1);
                offspringPopulationNext.Add(parent2);
            }


            // 3.���ׂĂ̎q�ɑ΂��ĕψٗ��ɉ����ēˑR�ψق�K�p���Ďq���̈�`�q��ύX
            foreach (var child in offspringPopulationNext)
            {
                Mutate(child);
            }

        }

        return offspringPopulationNext;
    }



    //�g�[�i�����g�I���@�T��I�����A���ꂩ����ł��ǂ����̂�I�Ԃ��̑I��
    private static Sample_Solution TournamentSelect(List<Sample_Solution> population)
    {
        int tournamentSize = 5; // �g�[�i�����g�̃T�C�Y
        List<Sample_Solution> tournament = new List<Sample_Solution>();

        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, population.Count);
            tournament.Add(population[randomIndex]);
        }

        // �ŗǂ̉���Ԃ��i�����ł͖ړI�֐��̍��v�l���ł��Ⴂ���̂��ŗǂƂ��܂��j
        return tournament.OrderBy(sol => sol.objectives.Sum()).FirstOrDefault();
    }



    ///����
    private static List<Sample_Solution> Crossover(Sample_Solution parent1, Sample_Solution parent2)
    {
        // �����ł͒P����1�_�������g�p���܂�
        Sample_Solution child1 = new Sample_Solution(numberOfVariables);
        Sample_Solution child2 = new Sample_Solution(numberOfVariables);

        int crossoverPoint = UnityEngine.Random.Range(0, numberOfVariables);

        for (int i = 0; i < numberOfVariables; i++)
        {
            if (i < crossoverPoint)
            {
                child1.variables[i] = parent1.variables[i];
                child2.variables[i] = parent2.variables[i];
            }
            else
            {
                child1.variables[i] = parent2.variables[i];
                child2.variables[i] = parent1.variables[i];
            }
        }

        return new List<Sample_Solution>() { child1, child2 };
    }


    //�ˑR�ψ�
    private static void Mutate(Sample_Solution solution)
    {
        for (int i = 0; i < numberOfVariables; i++)
        {
            if (UnityEngine.Random.value < mutationProbability)
            {
                // �����ł͒P���ȓˑR�ψق��g�p���A�ϐ��̒l�������_���ɕύX���܂�
                solution.variables[i] = UnityEngine.Random.Range(lowerBound, upperBound);
            }
        }
    }








    // ��x�z�\�����[�V�����𒊏o
    public static List<Sample_Solution> ExtractNewArchive(List<Sample_Solution> combinedPopulation)
    {
        // 1. ��x�z�\�[�g�����s���āA�e���̃����N���擾
        List<int> ranks = NonDominatedSorting.Sort(combinedPopulation);

        // 2. �ō������N���珇�ɁA�A�[�J�C�u��W�c���\�z
        
        int maxRank = ranks.Max();

        //�V�����A�[�J�C�u��W�c
        List<Sample_Solution> newArchivePopulation = new List<Sample_Solution>();
        for (int rank = 1; rank <= maxRank && newArchivePopulation.Count < populationSize; rank++)
        {
            List<Sample_Solution> currentFront = new List<Sample_Solution>();
            for (int i = 0; i < combinedPopulation.Count; i++)
            {
                if (ranks[i] == rank)
                {
                    currentFront.Add(combinedPopulation[i]);
                }
            }

            // 3. ���G�x���v�Z
            List<double> crowdingDistances = NonDominatedSorting.CalculateCrowdingDistance(currentFront);

            // 4. �A�[�J�C�u��W�c�ɉ���ǉ�
            if (newArchivePopulation.Count + currentFront.Count <= populationSize)
            {
                newArchivePopulation.AddRange(currentFront);
            }
            else
            {
                // ���G�x����������D�悵�Ēǉ�
                List<int> sortedIndices = crowdingDistances
                    .Select((value, index) => new { Value = value, Index = index })
                    .OrderByDescending(item => item.Value)
                    .Select(item => item.Index)
                    .ToList();

                for (int i = 0; i < sortedIndices.Count && newArchivePopulation.Count < populationSize; i++)
                {
                    newArchivePopulation.Add(currentFront[sortedIndices[i]]);
                }
            }
        }

        // newArchivePopulation �̃����N���Čv�Z
        List<int> newArchiveRanks = NonDominatedSorting.Sort(newArchivePopulation);
        for (int i = 0; i < newArchivePopulation.Count; i++)
        {
            newArchivePopulation[i].Rank = newArchiveRanks[i];
        }
    

        return newArchivePopulation;
    }




}
