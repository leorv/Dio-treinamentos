const students = [
    {
        name: 'Grace',
        grade: 7
    },
    {
        name: 'Jennifer',
        grade: 4
    },
    {
        name: 'Paul',
        grade: 8
    },
    {
        name: 'Sara',
        grade: 9
    }
];

function getApprovedStudents(studentsList){
    return studentsList.filter(students => students.grade >= 7);
}

console.log('Alunos aprovados: ');
console.log(getApprovedStudents(students));

console.log('\nLista de alunos:');
console.log(students);