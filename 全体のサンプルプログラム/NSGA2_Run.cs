using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


//���C���N���X
public class NSGA2_Run : MonoBehaviour
{
    public int populationSize = 100; //�i���T�C�Y
    public int generations = 10; //���㐔
    public float mutationProbability = 0.1f; //�ˑR�ψٗ�
    public float crossoverProbability = 0.9f; //������
    public int numberOfVariables = 12; // DTLZ2 for 3 objectives typically uses k=9 (12-3=9) decision variables
    public float lowerBound = 0f;
    public float upperBound = 1f;
    public float delay = 0.5f;

    public GameObject pointPrefab; // ������3D�I�u�W�F�N�g�i��F���j�̃v���n�u

    // �����������o���_���ꎞ�ۑ����郊�X�g
    private List<GameObject> instantiatedPoints = new List<GameObject>();

    //�S�̂̏����W��
    public static List<Sample_Solution> population;
    //�A�[�J�C�u��W�c
    public static List<Sample_Solution> archivePopulation = new List<Sample_Solution>(); // �A�[�J�C�u��W�c��ێ����郊�X�g
    //�q��W�c
    public static List<Sample_Solution> offspringPopulation;
    //���̕�W�c
    List<Sample_Solution> combinedPopulation;

    void Start()
    {
        //NSGA2�N���X�̃C���X�^���X��
        NSGAII nsga2 = new NSGAII(
            populationSize,
            generations,
            mutationProbability,
            crossoverProbability,
            numberOfVariables,
            lowerBound,
            upperBound);

        //NSGA2�̐i��(�i���݂̂��s���j
        //nsga2.Evolve();


        //�i���̎��o���i�i���Ǝ��o�����ǂ�����s���j
        StartCoroutine(RunAndVisualize());

        Debug.Log("�����ł������܂�");

    }

    //���ʂ̎��o�� //����I�ɂ�Evolve�֐��Ɠ������Ƃ�����
    public IEnumerator RunAndVisualize()
    {

        //�S�̂̏W��
        population = NSGAII.InitializePopulation();
        NSGAII.Evaluate(population);

        archivePopulation = population;
        //VisualizeParetoFront(NSGAII.archivePopulation);



        Debug.Log("�����烋�[�v�񂷂�");

        //�i���J�n
        for (int gen = 0; gen < NSGAII.generations; gen++)
        {
            
            // �A�[�J�C�u��W�c����ɃI�t�X�v�����O�𐶐�
            offspringPopulation = NSGAII.NextGeneration(archivePopulation);

            // �A�[�J�C�u��W�c�ƃI�t�X�v�����O������
            combinedPopulation = archivePopulation.Concat(offspringPopulation).ToList();

            // �V�����A�[�J�C�u��W�c�𒊏o
            NSGAII.Evaluate(combinedPopulation);
            archivePopulation = NSGAII.ExtractNewArchive(combinedPopulation);

            // ���݂̔�x�z�������o��
            VisualizeParetoFront(archivePopulation);
            //VisualizeParetoFront(combinedPopulation);

            // �A�[�J�C�u��W�c��V������W�c�Ƃ��Ďg�p
            population = archivePopulation;
            
            Debug.Log(gen + "����ڂ̌v�Z����");


            // �e����̏I���ɁA�����̒x������������
            yield return new WaitForSeconds(delay);

        }
    }

  
    //�A�[�J�C�u��W�c��\��
    public void VisualizeParetoFront(List<Sample_Solution> population)
    {

        // �ȑO�ɐ��������S�Ă̓_���폜
        foreach (var point in instantiatedPoints)
        {
            Destroy(point);
        }
        instantiatedPoints.Clear();

        int count = 1;
        foreach (Sample_Solution sol in population)
        {

            
            if (sol.Rank == 1)  // �����N��1�̏ꍇ�̂ݕ\��
            {
                //Debug.Log(count + ":�A�[�J�C�u�̃����N" + sol.Rank);
                count++;

                Vector3 position = new Vector3((float)sol.objectives[0], (float)sol.objectives[1], (float)sol.objectives[2]);
                GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
                point.GetComponent<Renderer>().material.color = Color.red;
                instantiatedPoints.Add(point); // ���̓_�����X�g�ɒǉ�
            }
            else
            {
                Vector3 position = new Vector3((float)sol.objectives[0], (float)sol.objectives[1], (float)sol.objectives[2]);
                GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
                point.GetComponent<Renderer>().material.color = Color.blue;
                instantiatedPoints.Add(point); // ���̓_�����X�g�ɒǉ�
            }
        }

    }
}





