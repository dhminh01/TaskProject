import { gql } from "@apollo/client";

export const CREATE_NEW_TASK = gql`
  mutation CreateTask($input: CreateTaskInput!) {
    createTask(input: $input) {
      task {
        id
        title
        description
        dueDate
        dateCreated
      }
    }
  }
`;

export const UPDATE_TASK = gql`
  mutation UpdateTask($input: UpdateTaskInput!) {
    updateTask(input: $input) {
      task {
        id
        title
        description
        dueDate
        dateCreated
      }
    }
  }
`;
