export type StatusNotaFiscal = 'Aberta' | 'Fechada';

export interface ItemNotaFiscal {
  produtoId: number;
  descricaoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: number;
  numero: number;
  status: StatusNotaFiscal;
  criadaEm: string;
  fechadaEm: string | null;
  itens: ItemNotaFiscal[];
}

export interface CriarNotaFiscalPayload {
  itens: ItemNotaFiscal[];
}

export interface ImprimirNotaResultado {
  sucesso: boolean;
  mensagem: string;
  nota: NotaFiscal | null;
}
