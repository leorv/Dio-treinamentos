# Common Table Expression

Forma auxiliar de organizar "statements", ou seja, blocos de códigos, para consultas muito grandes, gerando tabelas temporárias e criando relacionamentos entre elas.

Dentro dos statements podem ter SELECTs, INSERTs, UPDATEs ou DELETEs.

## WITH statements

```
SELECT numero, nome
FROM banco;

-- Fazer o acima usando statement:
WITH tbl_tmp_banco AS (
    SELECT numero, nome
    FROM banco
)
SELECT numero, nome
FROM tbl_tmp_banco;
```

Outro exemplo:

```
WITH params AS (
    SELECT 213 AS banco_numero
), tbl_tmp_banco AS (
    SELECT numero, nome
    FROM banco
    JOIN params ON params.banco_numero = banco.numero
)
SELECT numero, nome
FROM tbl_tmp_banco;
```

Dá para fazer o acima também com sub selects, mas pode ficar meio confuso.

```
SELECT banco.numero, banco.nome
FROM banco
JOIN (
    SELECT 213 AS banco_numero
) params ON params.banco_numero = banco.numero;
```

Até agora foram exemplos pequenos, mas dá pra colocar mais coisas:

```
WITH clientes_e_transacoes AS (
    SELECT  cliente.nome AS cliente_nome,
            tipo_transacao.nome AS tipo_transacao_nome,
            cliente_transacoes.valor AS tipo_transacao_valor
    FROM cliente_transacoes
    JOIN cliente ON cliente.numero = cliente_transacoes.cliente_numero
    JOIN tipo_transacao ON tipo_transacao.id = cliente_transacoes.tipo_transacao_id 
)
SELECT cliente_nome, tipo_transacao_nome, tipo_transacao_valor
FROM clientes_e_transacoes;
```

Melhorando ainda mais, vamos trazer somente as transações feitas no Itaú:

```
WITH clientes_e_transacoes AS (
    SELECT  cliente.nome AS cliente_nome,
            tipo_transacao.nome AS tipo_transacao_nome,
            cliente_transacoes.valor AS tipo_transacao_valor
    FROM cliente_transacoes
    JOIN cliente ON cliente.numero = cliente_transacoes.cliente_numero
    JOIN tipo_transacao ON tipo_transacao.id = cliente_transacoes.tipo_transacao_id 
    JOIN banco ON banco.numero = cliente_transacoes.banco_numero AND banco.nome ILIKE '%Itaú%'
)
SELECT cliente_nome, tipo_transacao_nome, tipo_transacao_valor
FROM clientes_e_transacoes;
```