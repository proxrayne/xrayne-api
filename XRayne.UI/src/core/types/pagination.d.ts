interface Pagination<T extends object> {
  items: T[];
  totalItems: number;
  currentPage: number;
  totalPages: number;
}

interface PaginationQuery {
  limit: number;
  page: number;
  search?: string;
}
