# Comandos avançados

## Views

São visões, "camadas" para as tabelas.

São "alias" para uma ou mais queries.

Aceitam comandos SELECT, INSERT, UPDATE e DELETE.

```
CREATE [ OR REPLACE ] [ TEMP | TEMPORARY ] [ RECURSIVE ] VIEW name [ ( column_name [, ...] ) ]
    [ WITH ( view_option_name [= view_option_value] [, ... ] ) ]
    AS query
    [ WITH [ CASCADED | LOCAL ] CHECK OPTION ]
```

https://www.postgresql.org/docs/12/sql-createview.html

Usar Views é considerado uma boa prática, pois quando foram querer usar o seu banco de dados, irão usar somente as Views, não tendo acesso direto ao banco de dados.

O comando INSERT, UPDATE e DELETE só pode ser usado se a View referencia uma única tabela, se houver joins, etc, será permitido somente o SELECT.

Cuidado com o comando `OR REPLACE` para não substituir um código que já existe e que você precisaria.

`TEMP | TEMPORARY` faz a view pertencer somente a sua sessão, se sair e entrar de novo, perdeu ela. Outras pessoas não tem acesso a essa view.

`RECURSIVE` é um SELECT dentro da própria view que chama ela mesma, até esgotar uma determinada opção.

`[ WITH [ CASCADED | LOCAL ] CHECK OPTION ]` são opções a serem usadas no INSERT, UPDATE e DELETE da view.

### Idempotência

```
CREATE OR REPLACE VIEW vw_bancos AS (
    SELECT numero, nome, ativo
    FROM banco
);

SELECT numero, nome, ativo
FROM vw_bancos;
```

```
CREATE OR REPLACE VIEW vw_bancos (banco_numero, banco_nome, banco_ativo) AS (
    SELECT numero, nome, ativo
    FROM banco
);

SELECT banco_numero, banco_nome, banco_ativo
FROM vw_bancos;
```

** Funcionam apenas para VIEWS com apenas uma tabela:**

```
INSERT INTO vw_bancos(numero, nome, ativo) VALUES (100, "Banco CEM", TRUE);
UPDATE vw_bancos SET nome = 'Banco 100' WHERE numero = 100;
DELETE FROM vw_bancos WHERE numero = 100;
```

### Recursive

```
CREATE OR REPLACE RECURSIVE VIEW (nome_da_view) (campos_da_view) AS (
    SELECT base
    UNION ALL
    SELECT campos
    FROM tabela_base
    JOIN (nome da view)
);
```

Obrigatória a existência de campos na view.
UNION ALL.

Exemplo:

```
CREATE TABLE IF NOT EXISTS funcionarios (
    id SERIAL NOT NULL,
    nome VARCHAR(50),
    gerente INTEGER,
    PRIMARY KEY(id),
    FOREIGN KEY (gerente) REFERENCES funcionarios (id)
);

INSERT INTO funcionarios (nome, gerente) VALUES ('Anselmo', null);
INSERT INTO funcionarios (nome, gerente) VALUES ('Beatriz', 1);
INSERT INTO funcionarios (nome, gerente) VALUES ('Magno', 1);
INSERT INTO funcionarios (nome, gerente) VALUES ('Cremilda', 2);
INSERT INTO funcionarios (nome, gerente) VALUES ('Wagner', 4);
INSERT INTO funcionarios (nome, gerente) VALUES ('Emengarda', 4);

CREATE OR REPLACE RECURSIVE VIEW vw_funcionarios(id, gerente, funcionario) AS (
    SELECT id, gerente, nome
    FROM funcionarios
    WHERE gerente IS NULL
    UNION ALL
    SELECT funcionarios.id, funcionarios.gerente, funcionarios.nome
    FROM funcionarios
    JOIN vw_funcionarios ON vw_funcionarios.id = funcionarios.gerente
);

SELECT id, gerente, funcionario
FROM vw_funcionarios;
```

Melhorando um pouco a view:

```
CREATE OR REPLACE RECURSIVE VIEW vw_funcionarios(id, gerente, funcionario) AS (
    SELECT id, CAST('' AS VARCHAR) AS gerente, nome
    FROM funcionarios
    WHERE gerente IS NULL
    UNION ALL
    SELECT funcionarios.id, gerentes.nome, funcionarios.nome
    FROM funcionarios
    JOIN vw_funcionarios ON vw_funcionarios.id = funcionarios.gerente
    JOIN funcionarios gerentes ON gerentes.id = vw_funcionarios.id
);

SELECT id, gerente, funcionario
FROM vw_funcionarios;
```

### WITH OPTIONS

Abaixo alguns exemplos e seus resultados.

```
CREATE OR REPLACE VIEW vw_banco AS (
    SELECT numero, nome, ativo
    FROM banco
);

INSERT INTO vw_bancos (numero, nome, ativo) VALUES (100, 'Banco CEM', false);
-- OK

CREATE OR REPLACE VIEW vw_bancos AS (
    SELECT numero, nome, ativo
    FROM banco
    WHERE ativo IS TRUE
) WITH LOCAL CHECK OPTION;

INSERT INTO vw_bancos (numero, nome, ativo) VALUES (100, 'Banco CEM', FALSE);
-- ERRO
```

Se tentar dar um INSERT, por exemplo, abaixo, vai tomar um erro, porque está tentando passar uma informação que é validada numa view dentro da outra.

```
CREATE OR REPLACE VIEW bancos_ativos AS (
    SELECT numero, nome, ativo
    FROM banco
    WHERE ativo IS TRUE
) WITH LOCAL CHECK OPTION;

CREATE OR REPLACE VIEW vw_bancos_maiores_que_100 AS (
    SELECT numero, nome, ativo
    FROM vw_banco
    WHERE numero > 100
) WITH LOCAL CHECK OPTION;

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (99, 'Banco DIO', FALSE);
-- ERRO

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (200, 'Banco DIO', FALSE);
-- ERRO
```

Como poderia fazer dar certo? Removendo o WITH LOCAL CHECK OPTIONS da primeira.

```
CREATE OR REPLACE VIEW bancos_ativos AS (
    SELECT numero, nome, ativo
    FROM banco
    WHERE ativo IS TRUE
);

CREATE OR REPLACE VIEW vw_bancos_maiores_que_100 AS (
    SELECT numero, nome, ativo
    FROM vw_banco
    WHERE numero > 100
) WITH LOCAL CHECK OPTION;

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (99, 'Banco DIO', FALSE);
-- ERRO

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (200, 'Banco DIO', FALSE);
-- OK
```

Se quiser validar as opções da primeira view também, sem usar o WITH LOCAL CHECK OPTIONS, substituir por WITH CASCADED CHECK OPTION.

```
CREATE OR REPLACE VIEW bancos_ativos AS (
    SELECT numero, nome, ativo
    FROM banco
    WHERE ativo IS TRUE
);

CREATE OR REPLACE VIEW vw_bancos_maiores_que_100 AS (
    SELECT numero, nome, ativo
    FROM vw_banco
    WHERE numero > 100
) WITH CASCADED CHECK OPTION;

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (99, 'Banco DIO', FALSE);
-- ERRO

INSERT INTO vw_bancos_maiores_que_100 (numero, nome, ativo) VALUES (200, 'Banco DIO', FALSE);
-- ERRO
```
